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
    public class InstructorController : ControllerBase
    {
        private string _connectionString;

        public InstructorController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        //------------------------------------------------------------------//
        // GET: api/Instructor get all instructors
        [HttpGet]

        public async Task<IActionResult> Get(string q)
        {
            if (q != null)
            {
                return await GetAllInstructorsWithQ(q);
            }
            else
            {
                return await GetAllInstructors();
            }
        }

        private async Task<IActionResult> GetAllInstructorsWithQ(string q)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id, i.FirstName, i.LastName, i.SlackHandle, i.Speciality, i.CohortId, c.Id, c.Name
                                          FROM Instructors i LEFT JOIN Cohorts C on i.CohortId = c.Id
                                          WHERE i.FirstName LIKE @q OR i.LastName LIKE @q OR i.SlackHandle LIKE @q";

                    cmd.Parameters.Add(new SqlParameter("@q", q));

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Instructor> instructors = new List<Instructor>();
                    Instructor instructor = null;

                    while (reader.Read())
                    {
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Speciality = reader.GetString(reader.GetOrdinal("Speciality")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            }

                        };

                        instructors.Add(instructor);
                    }
                    reader.Close();

                    return Ok(instructors);
                }
            }
        }

        private async Task<IActionResult> GetAllInstructors()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id, i.FirstName, i.LastName, i.SlackHandle, i.Speciality, i.CohortId, c.Id, c.Name
                                          FROM Instructors i LEFT JOIN Cohorts C on i.CohortId = c.Id";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Instructor> instructors = new List<Instructor>();
                    Instructor instructor = null;

                    while (reader.Read())
                    {
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Speciality = reader.GetString(reader.GetOrdinal("Speciality")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            }

                        };

                        instructors.Add(instructor);
                    }
                    reader.Close();

                    return Ok(instructors);
                }
            }
        }

        //------------------------------------------------------------------//
        // GET(id) Get one instructor

        [HttpGet("{id}")]
        public IActionResult GetOneInstructor([FromRoute] int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id, i.FirstName, i.LastName, i.SlackHandle, i.Speciality, i.CohortId, c.Id, c.Name
                                          FROM Instructors i LEFT JOIN Cohorts C on i.CohortId = c.Id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor instructor = null;

                    if(reader.Read())
                    {
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Speciality = reader.GetString(reader.GetOrdinal("Speciality")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            }
                        };
                    }

                    reader.Close();
                    return Ok(instructor);
                }
            }
        }

        //------------------------------------------------------------------//
        //Post a new instructor
        [HttpPost]
        public void AddNewInstructor([FromBody] Instructor newInstructor)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Instructor (FirstName, LastName, SlackHandle, Specialty, CohortId)
                                        VALUES (@firstname, @lastname, @slackhandle, @specialty, @cohortId)";
                    cmd.Parameters.Add(new SqlParameter("@firstname", newInstructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", newInstructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackhandle", newInstructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@specialty", newInstructor.Speciality));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", newInstructor.CohortId));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        //------------------------------------------------------------------//
        //Edit an existing instructor listing
        [HttpPut("{id}")]
        public void UpdateInstructor([FromRoute] int id, [FromBody] Instructor updatedInstructor)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Instructors
                                           SET FirstName = @firstname, LastName = @lastname, SlackHandle = @slackhandle,
                                               Speciality = @specialty, CohortId = @cohortid
                                         WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@firstName", updatedInstructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", updatedInstructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", updatedInstructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@specialty", updatedInstructor.Speciality));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", updatedInstructor.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();

                }
            }
        }
        //------------------------------------------------------------------//
        //Delete an existing instructor listing
        [HttpDelete("{id}")]
        public void DeleteInstructor([FromRoute] int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Instructor WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

}