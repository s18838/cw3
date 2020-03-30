CREATE PROCEDURE PromoteStudents @Studies VARCHAR(100), @Semester INT
AS
BEGIN

    DECLARE @IdStudy INT = (SELECT IdStudy FROM Studies WHERE Name=@Studies);
    IF @IdStudy IS NULL
        BEGIN
            THROW 51000, 'The record does not exist.', 1;
        END;

    DECLARE @IdEnrollment INT = (SELECT IdEnrollment FROM Enrollment WHERE Semester = @Semester + 1 AND IdStudy = @IdStudy);
    IF @IdEnrollment IS NULL
        BEGIN
            INSERT INTO Enrollment SELECT NULLIF(MAX(E.IdEnrollment) + 1, 0), @Semester + 1, @IdStudy, GETDATE() FROM Enrollment E;
            SET @IdEnrollment = (SELECT IdEnrollment FROM Enrollment WHERE Semester = @Semester + 1 AND IdStudy = @IdStudy);
        END;

    DECLARE IndexNumber_cursor CURSOR
        FOR SELECT IndexNumber FROM Student JOIN Enrollment ON Enrollment.IdEnrollment = Student.IdEnrollment WHERE Semester = @Semester AND IdStudy = @IdStudy

    OPEN IndexNumber_cursor;

    DECLARE @IndexNumber VARCHAR(100);

    FETCH NEXT FROM IndexNumber_cursor
        INTO @IndexNumber;

    WHILE @@FETCH_STATUS = 0
        BEGIN

            UPDATE Student SET IdEnrollment = @IdEnrollment WHERE IndexNumber = @IndexNumber;

            FETCH NEXT FROM IndexNumber_cursor
                INTO @IndexNumber;
        END
    CLOSE IndexNumber_cursor;
    DEALLOCATE IndexNumber_cursor;
END;