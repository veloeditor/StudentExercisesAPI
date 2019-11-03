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
    public class StudentController : ControllerBase
    {
        private string _connectionString;

        public StudentController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        //------------------------------------------------------------------//
        //Routing query based on if they select to see exercises or not

        [HttpGet]
        public async Task<IActionResult> Get(string q, string include)
        {
            if (q != null && include != null)
            {
                return await GetStudentsWithExercisesQ(q, include);
            }
            else if (q != null && include == null)
            {
                return await GetAllStudentsWithQ(q);
            }
            else if (q == null && include != null)
            {
                return await GetStudentsWithExercises(include);
            }
            else
            {
                return await GetAllStudents();
            }
        }

        //------------------------------------------------------------------//
        // GET: api/Student get all students
        
        private async Task<IActionResult> GetAllStudents()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Name
                                          FROM Students s LEFT JOIN Cohorts c on s.CohortId = c.Id";

                    SqlDataReader reader = cmd.ExecuteReader();
                    Dictionary<int, Student> students = new Dictionary<int, Student>();

                    while (reader.Read())
                    {
                        int studentId = reader.GetInt32(reader.GetOrdinal("Id"));
                        Student newStudent = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Exercises = new List<Exercise>(),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            }
                        };
                        students.Add(studentId, newStudent);
                    }
                    reader.Close();
                    return Ok(students.Values);
                }
            }
        }

        //------------------------------------------------------------------//
        // GET: api/Student get all students with q parameter

        private async Task<IActionResult> GetAllStudentsWithQ(string q)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))

            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Id, c.Name 
                                          FROM Students s LEFT JOIN Cohorts c ON s.CohortId = c.Id
                                         WHERE s.FirstName LIKE @q OR s.LastName LIKE @q OR s.SlackHandle LIKE @q";
                    cmd.Parameters.Add(new SqlParameter("@q", q));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;

                    if (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Exercises = new List<Exercise>(),
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
                    return Ok(student);
                }
            }
        }

        //------------------------------------------------------------------//
        // GET: api/Student get all students WITH exercises with q and include

        private async Task<IActionResult> GetStudentsWithExercisesQ(string q, string include)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include.ToLower() == "exercise")
                    {
                        cmd.CommandText = @"SELECT s.Id as StudentId, s.FirstName, s.LastName, s.SlackHandle, s.CohortId,
                                                   c.Id as CohortId, c.Name as CohortName,
                                                   se.ExerciseId, e.Name as ExerciseName, e.CodeLanguage
                                              FROM Students s LEFT JOIN Cohorts c ON s.CohortId = c.Id
                                              LEFT JOIN StudentExercises se ON se.StudentId = s.Id
                                              LEFT Join Exercises e ON se.ExerciseId = e.Id
                                             WHERE s.FirstName LIKE @q OR s.LastName LIKE @q OR s.SlackHandle LIKE @q"; ;
                        
                        cmd.Parameters.Add(new SqlParameter("@q", q));
                        SqlDataReader reader = cmd.ExecuteReader();

                        Dictionary<int, Student> students = new Dictionary<int, Student>();
                        while (reader.Read())
                        {
                            int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                            if (!students.ContainsKey(studentId))
                            {
                                Student newStudent = new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                    Exercises = new List<Exercise>(),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Cohort = new Cohort
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                        Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                        Students = new List<Student>(),
                                        Instructors = new List<Instructor>()
                                    }
                                };
                                students.Add(studentId, newStudent);
                            }
                            Student fromDictionary = students[studentId];

                            if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                            {
                                Exercise exercise = new Exercise
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    CodeLanguage = reader.GetString(reader.GetOrdinal("CodeLanguage"))
                                };
                                fromDictionary.Exercises.Add(exercise);
                            }
                        }

                        reader.Close();
                        return Ok(students.Values);
                    }
                    return null;
                }
            }
        }


        //------------------------------------------------------------------//
        // GET: api/Student get all students WITH exercises

        private async Task<IActionResult> GetStudentsWithExercises(string include)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include.ToLower() == "exercise")
                    {
                        cmd.CommandText = @"SELECT s.Id as StudentId, s.FirstName, s.LastName, s.SlackHandle, s.CohortId,
                                                   c.Id as CohortId, c.Name as CohortName,
                                                   se.ExerciseId, e.Name as ExerciseName, e.CodeLanguage
                                              FROM Students s LEFT JOIN Cohorts c ON s.CohortId = c.Id
                                              LEFT JOIN StudentExercises se ON se.StudentId = s.Id
                                              LEFT Join Exercises e ON se.ExerciseId = e.Id";
                        SqlDataReader reader = cmd.ExecuteReader();

                        Dictionary<int, Student> students = new Dictionary<int, Student>();
                        while(reader.Read())
                        {
                            int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                            if (! students.ContainsKey(studentId))
                            {
                                Student newStudent = new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                    Exercises = new List<Exercise>(),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Cohort = new Cohort
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                        Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                        Students = new List<Student>(),
                                        Instructors = new List<Instructor>()
                                    }
                                };
                                students.Add(studentId, newStudent);
                            }
                            Student fromDictionary = students[studentId];

                            if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                            {
                                Exercise exercise = new Exercise
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    CodeLanguage = reader.GetString(reader.GetOrdinal("CodeLanguage"))
                                };
                                fromDictionary.Exercises.Add(exercise);
                            }
                        }

                        reader.Close();
                        return Ok(students.Values);
                    }
                    return null;
                }
            }
        }

        //------------------------------------------------------------------//
        // GET: api/Student get one student
        [HttpGet("{id}")]

        public async Task<IActionResult> Get([FromRoute] int id, string include)
        {
            if (include != null)
            {
                return await GetOneStudentWithExcerises(id, include);
            }
            else
            {
                return await GetStudent(id);
            }
        }

        private async Task<IActionResult> GetStudent([FromRoute] int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Id, C.Name" +
                        "                 FROM Students s INNER JOIN Cohorts c ON s.CohortId = c.Id WHERE s.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;

                    if (reader.Read())
                    {
                        student = new Student()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Exercises = new List<Exercise>(),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>(),

                            }
                        };
                    }

                    reader.Close();

                    return Ok(student);
                }
            }
        }

        private async Task<IActionResult> GetOneStudentWithExcerises(int id, string include)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    if (include.ToLower() == "exercise")
                    {
                        cmd.CommandText = @"SELECT s.Id as StudentId, s.FirstName, s.LastName, s.SlackHandle, s.CohortId,
                                                   c.Id as CohortId, c.Name as CohortName, 
                                                   se.ExerciseId, e.Name as ExerciseName, e.CodeLanguage
                                              FROM Students s LEFT JOIN Cohorts c ON s.CohortId = c.Id
                                              LEFT JOIN StudentExercises se ON se.StudentId = s.Id
                                              LEFT JOIN Exercises e ON se.ExerciseId = e.Id
                                              WHERE s.Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();

                        Dictionary<int, Student> students = new Dictionary<int, Student>();
                        while (reader.Read())
                        {
                            int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                            if (!students.ContainsKey(studentId))
                            {
                                Student newStudent = new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                    Exercises = new List<Exercise>(),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Cohort = new Cohort
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                        Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                        Students = new List<Student>(),
                                        Instructors = new List<Instructor>()
                                    }
                                };

                                students.Add(studentId, newStudent);
                            }

                            Student fromDictionary = students[studentId];

                            if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                            {
                                Exercise exercise = new Exercise
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    CodeLanguage = reader.GetString(reader.GetOrdinal("CodeLanguage"))
                                };
                                fromDictionary.Exercises.Add(exercise);
                            }

                        }

                        reader.Close();
                        return Ok(students.Values);
                    }

                    return null;
                }
            }

        }


        //------------------------------------------------------------------//
        //Post add a new student
        [HttpPost]
        public void PostNewStudent([FromBody] Student newStudent)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Students (FirstName, LastName, SlackHandle, CohortId)
                                        VALUES (@firstname, @lastname, @slackhandle, @cohortId)";
                    cmd.Parameters.Add(new SqlParameter("@firstname", newStudent.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", newStudent.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackhandle", newStudent.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", newStudent.CohortId));

                    cmd.ExecuteNonQuery();

                }
            }
        }

        //------------------------------------------------------------------//
        //Put update a student listing
        [HttpPut("{id}")]
        public void UpateStudentListing([FromRoute] int id, [FromBody] Student updatedStudent)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Students
                                           SET FirstName = @firstname, LastName = @lastname,
                                               SlackHandle = @slackhandle, CohortId = @cohortId
                                         WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@firstname", updatedStudent.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", updatedStudent.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackhandle", updatedStudent.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", updatedStudent.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //------------------------------------------------------------------//
        //Delete a student listing
        [HttpDelete("{id}")]
        public void DeleteStudentListing([FromRoute] int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Students WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();
                }
            }
        }


    }
}
