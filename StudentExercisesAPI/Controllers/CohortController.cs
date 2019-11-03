using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercisesAPI.Models;

namespace StudentExercisesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CohortController : ControllerBase
    {
        private string _connectionString;

        public CohortController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        //------------------------------------------------------------------//
        // GET: api/Get all cohorts
        [HttpGet]

        public async Task<IActionResult> Get(string q)
        {
            if (q != null)
            {
                return await GetAllTheCohortsWithQ(q);
            }
            else
            {
                return await GetAllTheCohorts();
            }
        }

        private async Task<IActionResult> GetAllTheCohortsWithQ(string q)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id as CohortId, c.Name as CohortName, 
                                            s.Id as StudentId, s.FirstName as StudentFirstName, s.LastName as StudentLastName, s.SlackHandle as StudentSlackHandle,
                                            i.Id as InstructorId, i.FirstName as InstructorFirstName, i.LastName as InstructorLastName, i.SlackHandle as InstructorSlackHandle, i.Speciality as InstructorSpecialty
                                        FROM Cohorts c LEFT JOIN Students s on s.CohortId = c.Id 
                                        LEFT JOIN Instructors i ON  i.CohortId = c.Id
                                      WHERE c.Name LIKE @q";

                    cmd.Parameters.Add(new SqlParameter("@q", q));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, Cohort> cohorts = new Dictionary<int, Cohort>();

                    while (reader.Read())
                    {
                        int cohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        if (!cohorts.ContainsKey(cohortId))
                        {
                            Cohort cohort = new Cohort
                            {
                                Id = cohortId,
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            };
                            cohorts.Add(cohortId, cohort);
                        }
                        Cohort fromDictionary = cohorts[cohortId];

                        if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                        {
                            Student student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Exercises = new List<Exercise>()
                            };
                            fromDictionary.Students.Add(student);
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                        {
                            int checkDuplicateInstructor = reader.GetInt32(reader.GetOrdinal("InstructorId"));

                            if (!cohorts[cohortId].Instructors.Any(i => i.Id == checkDuplicateInstructor))
                            {
                                Instructor instructor = new Instructor
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                    Speciality = reader.GetString(reader.GetOrdinal("InstructorSpecialty")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Cohort = null
                                };

                                fromDictionary.Instructors.Add(instructor);

                            }
                        }

                    }
                    reader.Close();
                    return Ok(cohorts.Values);
                }
            }
        }

        public async Task<IActionResult> GetAllTheCohorts()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id as CohortId, c.Name as CohortName, 
                                            s.Id as StudentId, s.FirstName as StudentFirstName, s.LastName as StudentLastName, s.SlackHandle as StudentSlackHandle,
                                            i.Id as InstructorId, i.FirstName as InstructorFirstName, i.LastName as InstructorLastName, i.SlackHandle as InstructorSlackHandle, i.Speciality as InstructorSpecialty
                                        FROM Cohorts c LEFT JOIN Students s on s.CohortId = c.Id 
                                        LEFT JOIN Instructors i ON  i.CohortId = c.Id";

                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, Cohort> cohorts = new Dictionary<int, Cohort>();

                    while (reader.Read())
                    {
                        int cohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        if (!cohorts.ContainsKey(cohortId))
                        {
                            Cohort cohort = new Cohort
                            {
                                Id = cohortId,
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            };
                            cohorts.Add(cohortId, cohort);
                        }
                        Cohort fromDictionary = cohorts[cohortId];

                        if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                        {
                            Student student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Exercises = new List<Exercise>()
                            };
                            fromDictionary.Students.Add(student);
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                        {
                            int checkDuplicateInstructor = reader.GetInt32(reader.GetOrdinal("InstructorId"));

                            if(!cohorts[cohortId].Instructors.Any(i => i.Id == checkDuplicateInstructor))
                            {
                            Instructor instructor = new Instructor
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                Speciality = reader.GetString(reader.GetOrdinal("InstructorSpecialty")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = null
                            };                           
                        
                            fromDictionary.Instructors.Add(instructor);
                           
                           }
                        }

                    }
                    reader.Close();
                    return Ok(cohorts.Values);
                }
            }
        }



        //Get - get a single cohort

        [HttpGet("{id}")]
        public async Task<IActionResult> GetACohort([FromRoute] int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id as CohortId, c.Name as CohortName, 
                                            s.Id as StudentId, s.FirstName as StudentFirstName, s.LastName as StudentLastName, s.SlackHandle as StudentSlackHandle,
                                            i.Id as InstructorId, i.FirstName as InstructorFirstName, i.LastName as InstructorLastName, i.SlackHandle as InstructorSlackHandle, i.Speciality as InstructorSpecialty
                                        FROM Cohorts c LEFT JOIN Students s on s.CohortId = c.Id 
                                        LEFT JOIN Instructors i ON  i.CohortId = c.Id
                                       WHERE c.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, Cohort> cohorts = new Dictionary<int, Cohort>();

                    while (reader.Read())
                    {
                        int cohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        if (!cohorts.ContainsKey(cohortId))
                        {
                            Cohort cohort = new Cohort
                            {
                                Id = cohortId,
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            };
                            cohorts.Add(cohortId, cohort);
                        }
                        Cohort fromDictionary = cohorts[cohortId];

                        if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                        {
                            Student student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Exercises = new List<Exercise>()
                            };
                            fromDictionary.Students.Add(student);
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                        {
                            Instructor instructor = new Instructor
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                Speciality = reader.GetString(reader.GetOrdinal("InstructorSpecialty")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = null
                            };
                            fromDictionary.Instructors.Add(instructor);
                        }

                    }
                    reader.Close();
                    return Ok(cohorts.Values);
                }
            }
        }

        //Post - post a new cohort to the database
        [HttpPost]
        public void PostNewCohort([FromBody] Cohort newCohort)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))

            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Cohorts (Name) VALUES (@name)";
                    cmd.Parameters.Add(new SqlParameter("@name", newCohort.Name));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //Put - edit an existing cohort in the database
        [HttpPut("{id}")]
        public void UpdateACohort([FromRoute] int id, Cohort updatedCohort)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Cohorts 
                                           SET Name = @name
                                         WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@name", updatedCohort.Name));
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //Delete - delete an existing cohort
        [HttpDelete("{id})")]
        public void DeleteACohort([FromRoute] int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))

            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Cohorts WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
    }
