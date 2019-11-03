SELECT c.Id as TheCohortId, c.Name as CohortName, 
                                            s.Id as TheStudentId, s.FirstName as StudentFirstName, s.LastName as StudentLastName, s.SlackHandle as StudentSlackHandle,
                                            i.Id as TheInstructorId, i.FirstName as InstructorFirstName, i.LastName as InstructorLastName, i.SlackHandle as InstructorSlackHandle, i.Speciality as InstructorSpecialty
                                        FROM Cohorts c LEFT JOIN Students s on s.CohortId = c.Id 
                                        LEFT JOIN Instructors i ON  i.CohortId = c.Id