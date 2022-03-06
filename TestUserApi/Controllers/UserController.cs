using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TestUserApi.Data;
using TestUserApi.Models;
using TestUserApi.Models.DTO;

namespace TestUserApi.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        // DBContext for CRUD        
        private readonly DatabaseContext _databaseContext;

        // Inject DBContext 
        public UserController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        // Function to check for null fields
        private static bool CheckFields(UserModel model)
        {
            Console.WriteLine(model);
            if (model.Email == "" ||
                model.Name == "" ||
                model.DocTypeId == -1 ||
                model.DocNumber == "" ||
                model.Lastname == "" ||
                model.Password == "")
            {
                return false;
            }
            return true;
        }

        // Check valid mail 
        private bool CheckMail(string email)
        {
            if (email.EndsWith("."))
            {
                return false;
            }
            try
            {
                if (Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
                {
                    var addr = new System.Net.Mail.MailAddress(email);
                    return addr.Address == email;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        // Check password length
        private bool CheckPassword(string password)
        {
            return password.Length > 8;
        }

        // Function to generate the details of bad request
        private string GenerateBadRequestMsg(UserModel model)
        {
            string msg = "";
            int i = 1;
            if (!CheckFields(model))
            {
                msg = i + ". Check for null fields\n";
                i++;

            }
            if (!CheckPassword(model.Password))
            {
                msg = msg + i + ". Password must be 8 characters long\n";
                i++;
            }
            if (!CheckMail(model.Email))
            {
                msg = msg + i + ". Check the Email format\n";
            }
            return msg;
        }


        // Function to return DTOs
        private static UserDTO ModelToDTO(UserModel model) =>
            new UserDTO
            {
                Id = model.Id,
                Name = model.Name,
                Lastname = model.Lastname,
                Email = model.Email,
                DocTypeId = model.DocTypeId,
                DocNumber = model.DocNumber
            };


        // Get all users from db
        [HttpGet]
        public async Task<ActionResult<List<UserDTO>>> GetUsers()
        {
            try
            {
                // Get users and use the DTO to prevent password exposure
                List<UserDTO> users = await _databaseContext.Users
                    .Select(user => ModelToDTO(user))
                    .ToListAsync();

                if (users == null)
                {
                    string msg = "No users in DataBase";
                    return NotFound(msg);
                }

                return Ok(users);
            }

            catch (Exception ex)
            {
                string msg = "Something went wrong. \n Error details: " + ex.Message;
                return StatusCode(500, msg);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            try
            {
                var temp = await _databaseContext.Users.FindAsync(id);

                if (temp == null)
                {
                    string msg = "No user with id: " + id;
                    return NotFound(msg);
                }

                return Ok(ModelToDTO(temp));
            }

            catch (Exception ex)
            {
                string msg = "Something went wrong. \n Error details: " + ex.Message;
                return StatusCode(500, msg);
            }
        }

        // Log in user
        [HttpGet("login/{docNum}")]
        public async Task<ActionResult<UserModel>> getUserCredentials(string docNum)
        {
            try
            {
                var temp = await _databaseContext.Users.Where(user =>
                   user.DocNumber == docNum
                ).ToListAsync();

                if (temp == null)
                {
                    string msg = "No user with DocNum: " + docNum;
                    return NotFound(msg);
                }

                UserModel user = temp.First();
                return Ok(user);

            }

            catch (Exception ex)
            {
                string msg = "Something went wrong. \n Error details: " + ex.Message;
                return StatusCode(500, msg);
            }
        }

        [HttpGet("getDocTypes")]
        public async Task<ActionResult<DocTypeModel>> GetDoctypes()
        {
            try
            {
                var doctypes = await _databaseContext.DocTypes.ToListAsync();
                return Ok(doctypes);
            }

            catch (Exception ex)
            {
                string msg = "Something went wrong. \n Error details: " + ex.Message;
                return StatusCode(500, msg);
            }
           
        }

        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostUser(UserModel model)
        {
            try
            {
                //Check if document is alredy registered
                var temp = await _databaseContext.Users.Where(user =>
                  user.DocNumber == model.DocNumber
                ).ToListAsync();

                if (temp.Count == 0)
                {
                    if (CheckFields(model) && CheckPassword(model.Password) && CheckMail(model.Email))
                    {
                        // Hash password for secure storage
                        string password = model.Password;
                        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                        model.Password = passwordHash;

                        // Check if doctype exist
                        var doctype = await _databaseContext.DocTypes.FindAsync(model.DocTypeId);

                        if (doctype == null)
                        {
                            return BadRequest("Doctype doesnt exist");
                        }

                        // Save user in db
                        _databaseContext.Users.Add(model);
                        await _databaseContext.SaveChangesAsync();
                        return Ok(ModelToDTO(model));
                    }

                    else
                    {
                        string msg = GenerateBadRequestMsg(model);
                        return BadRequest(msg);
                    }
                }
                else
                {
                    return BadRequest("DocNum alredy registered");
                }
            }
            catch (Exception ex)
            {
                string msg = "Something went wrong. \n Error details: " + ex.Message;
                return StatusCode(500, msg);
            }
        }



        [HttpPut("{id}")]
        public async Task<ActionResult<List<UserDTO>>> PutUser(int id, UserModel model)
        {
            try
            {
                var modifiedUser = await _databaseContext.Users.FindAsync(id);
                if (modifiedUser == null)
                {
                    string msg = "No user with id: " + id;
                    return NotFound(msg);
                }

                // Check if doctype exist
                var doctype = await _databaseContext.DocTypes.FindAsync(model.DocTypeId);

                if (doctype == null)
                {
                    return BadRequest("Doctype doesnt exist");
                }

                if (CheckFields(model) && CheckPassword(model.Password) && CheckMail(model.Email))
                {

                    modifiedUser.Name = model.Name;
                    modifiedUser.Lastname = model.Lastname;
                    modifiedUser.DocTypeId = model.DocTypeId;
                    modifiedUser.DocNumber = model.DocNumber;
                    modifiedUser.Email = model.Email;

                    // Check for changes in password to avoid re hasing
                    string password = model.Password;
                    if (!(BCrypt.Net.BCrypt.Verify(password, modifiedUser.Password) || modifiedUser.Password == password))
                    {
                        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                        modifiedUser.Password = passwordHash;
                    }

                    await _databaseContext.SaveChangesAsync();

                    return Ok(ModelToDTO(modifiedUser));
                }
                else
                {
                    string msg = GenerateBadRequestMsg(model);

                    return BadRequest(msg);
                }
            }
            catch (Exception ex)
            {
                string msg = "Something went wrong. \n Error details: " + ex.Message;
                return StatusCode(500, msg);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<UserDTO>> DeleteUser(int id)
        {
            try
            {
                var deletedUser = await _databaseContext.Users.FindAsync(id);
                if (deletedUser == null)
                {
                    string msg = "No user with id: " + id;
                    return NotFound(msg);
                }

                _databaseContext.Users.Remove(deletedUser);
                await _databaseContext.SaveChangesAsync();

                return Ok(ModelToDTO(deletedUser));
            }
            catch (Exception ex)
            {
                string msg = "Something went wrong. \n Error details: " + ex.Message;
                return StatusCode(500, msg);
            }
        }
    }
}
