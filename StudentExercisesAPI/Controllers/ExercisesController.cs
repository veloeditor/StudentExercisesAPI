  using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
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
    public class ExercisesController : ControllerBase
    {
        private string _connectionString;

        public ExercisesController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        //------------------------------------------------------------------//
        //Routing query based on q or include

        [HttpGet]
        public async Task<IActionResult> Get(string q, string include)
        {
            if (q != null && include != null)
            {
                return await GetAllExercisesIncludeQ(q, include);
            }
            else if (q != null && include == null)
            {
                return await GetAllExercisesQ(q);
            }
            else if (q == null && include != null)
            {
                return await GetExerciesWithIncludeStudents(include);
            }
            else
            {
                return await GetAllExercises();
            }
        }

        // Get all the exercises with no inputs (q or include)
        private async Task<IActionResult> GetAllExercises()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, CodeLanguage FROM Exercises";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Exercise> exercises = new List<Exercise>();
                    Exercise exercise = null;

                    while (reader.Read())
                    {
                        exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            CodeLanguage = reader.GetString(reader.GetOrdinal("CodeLanguage")),
                            Students = new List<Student>()
                        };

                        exercises.Add(exercise);
                    }
                    reader.Close();

                    return Ok(exercises);
                }
            }
        }

        /// Get all exercies with string q as argument
        private async Task<IActionResult> GetAllExercisesQ(string q)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, CodeLanguage FROM Exercises
                                        WHERE Name LIKE @q OR CodeLanguage LIKE @q";
                    cmd.Parameters.Add(new SqlParameter("@q", q));
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Exercise> exercises = new List<Exercise>();
                    Exercise exercise = null;

                    while (reader.Read())
                    {
                        exercise = new Exercise
                        {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Name =  reader.GetString(reader.GetOrdinal("Name")),
                        CodeLanguage = reader.GetString(reader.GetOrdinal("CodeLanguage"))
                    };
                    exercises.Add(exercise);
                        
                            
                    }
                    reader.Close();

                    return Ok(exercises);

                }
            }
        }

        /// Get all exercies with string q AND incloude as arguments

        private async Task<IActionResult> GetAllExercisesIncludeQ(string q, string include)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include.ToLower() == "student")
                    {
                        cmd.CommandText = @"SELECT e.Id as ExerciseId, e.Name as ExerciseName, e.CodeLanguage,
		                                            s.Id as StudentId, s.FirstName, s.LastName, s.SlackHandle, s.CohortId
                                              FROM Exercises e
                                              LEFT JOIN StudentExercises se ON se.ExerciseId = e.Id
				                              LEFT JOIN Students s on se.StudentId = s.Id
                                             WHERE Name LIKE @q OR CodeLanguage LIKE @q";

                        cmd.Parameters.Add(new SqlParameter("@q", q));
                        SqlDataReader reader = cmd.ExecuteReader();

                        Dictionary<int, Exercise> exercises = new Dictionary<int, Exercise>();
                        while (reader.Read())
                        {
                            int exerciseId = reader.GetInt32(reader.GetOrdinal("ExerciseId"));
                            if (!exercises.ContainsKey(exerciseId))
                            {
                                Exercise exercise = new Exercise
                                {
                                    Id = exerciseId,
                                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    CodeLanguage = reader.GetString(reader.GetOrdinal("CodeLanguage"))
                                };

                                exercises.Add(exerciseId, exercise);
                            }

                            Exercise fromDictionary = exercises[exerciseId];
                            if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                            {
                                Student student = new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Cohort = null
                                };

                                fromDictionary.Students.Add(student);
                            }

                        }
                        reader.Close();

                        return Ok(exercises.Values);
                    }

                    return null;

                }
            }
        }

        // Get all exercises and include students
        private async Task<IActionResult> GetExerciesWithIncludeStudents(string include)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include.ToLower() == "student")
                    {
                        cmd.CommandText = @"SELECT e.Id as 'ExerciseId', e.Name as 'ExerciseName', e.CodeLanguage,
		                                            s.Id as 'StudentId', s.FirstName, s.LastName, s.SlackHandle, s.CohortId
                                              FROM Exercises e 
                                              LEFT JOIN StudentExercises se ON se.ExerciseId = e.Id
				                              LEFT JOIN Students s on se.StudentId = s.Id";

                        SqlDataReader reader = cmd.ExecuteReader();

                        Dictionary<int, Exercise> exercises = new Dictionary<int, Exercise>();
                        while (reader.Read())
                        {
                            int exerciseId = reader.GetInt32(reader.GetOrdinal("ExerciseId"));
                            if (!exercises.ContainsKey(exerciseId))
                            {
                                Exercise exercise = new Exercise
                                {
                                    Id = exerciseId,
                                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    CodeLanguage = reader.GetString(reader.GetOrdinal("CodeLanguage"))
                                };

                                exercises.Add(exerciseId, exercise);
                            }

                            Exercise fromDictionary = exercises[exerciseId];
                            if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                            {
                                Student student = new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Cohort = null
                                };

                                fromDictionary.Students.Add(student);
                            }

                        }
                        reader.Close();

                        return Ok(exercises.Values);
                    }

                    return null;

                }
            }
        }

        ///  Get a single exercise from the database
        [HttpGet("{id}")]
        public IActionResult GetExercise([FromRoute] int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, CodeLanguage FROM Exercises WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Exercise exercise = null;

                    if (reader.Read())
                    {
                        exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            CodeLanguage = reader.GetString(reader.GetOrdinal("CodeLanguage"))
                        };

                    }

                    reader.Close();
                    return Ok(exercise);
                }
            }
        }

        // Post a single exercise
        [HttpPost]
        public void PostExercise([FromBody] Exercise newExercise)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Exercises (Name, CodeLanguage) VALUES (@name, @language)";
                    cmd.Parameters.Add(new SqlParameter("@name", newExercise.Name));
                    cmd.Parameters.Add(new SqlParameter("@language", newExercise.CodeLanguage));
                    cmd.ExecuteNonQuery();
                }

            }
        }

        //Put - update an exercise entry in the database
        [HttpPut("{id}")]
        public void UpdateAnExercise([FromRoute] int id, [FromBody] Exercise updatedExercise)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Exercise
                                           SET Name = @name, CodeLanguage = @language
                                         WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@name", updatedExercise.Name));
                    cmd.Parameters.Add(new SqlParameter("@language", updatedExercise.CodeLanguage));
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // DELETE: api/exercise/3
        [HttpDelete("{id}")]
        public void DeleteAnExercise([FromRoute] int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Exercise WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();
                }
            }
        }


    }
}
