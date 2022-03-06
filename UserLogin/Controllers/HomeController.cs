using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using UserAuth.Models;
using UserLogin.Models;

namespace UserLogin.Controllers
{
    public class HomeController : Controller
    {
        ModelLoader md = new ModelLoader();

        public ActionResult Index()
        {
            return View(md);
        }

        private async Task<List<DocType>> GetDocTypes()
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("https://localhost:7256/api/User/getDocTypes"))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<DocType>>(apiResponse);
                    }
                    return new List<DocType>();
                }
            }
        }

        // Log user in
        [HttpPost]
        public async Task<IActionResult> Index(string docNumInput, string passwordInput)
        {
            md = new ModelLoader();
            if (ValidateLoginInputs(docNumInput, passwordInput))
            {
                //get DocTypes to show in table
                md.docTypeModel = await GetDocTypes();

                UserModel user = new UserModel();
                using (var httpClient = new HttpClient())
                {
                    string uri = "https://localhost:7256/api/User/login/" + docNumInput;
                    using (var response = await httpClient.GetAsync(uri))
                    {
                        // check if user exists
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            user = JsonConvert.DeserializeObject<UserModel>(apiResponse);
                            //Check user password
                            if (BCrypt.Net.BCrypt.Verify(passwordInput, user.Password))
                            {
                                md.userModel = user;
                                ViewData["docName"] = md.docTypeModel.Where(doc => user.DocTypeId == doc.Id).First().Name;
                                ViewData["LoginBadPassword"] = null;
                                return View(md);
                            }
                            else
                            {
                                ViewData["LoginBadPassword"] = "Datos incorrectos";
                                md.userModel = null;
                                return View(md);
                            }
                        }
                        else
                            return View(md);
                    }
                }
            }
            else
            {
                return View(md);
            }
        }

        // Look for empty fields in login form
        private bool ValidateLoginInputs(string docNumInput, string passwordInput)
        {
            bool validationOk = true;
            if (docNumInput == null)
            {
                validationOk = false;
                ViewData["LoginDocNumber"] = "Por favor ingrese un documento";
            }
            else
            {
                ViewData["LoginDocNumber"] = null;
            }
            if (passwordInput == null)
            {
                validationOk = false;
                ViewData["LoginPasword"] = "Por favor ingrese un documento";
            }
            else
            {
                ViewData["LoginPasword"] = null;
            }
            return validationOk;
        }

        // Look for empty and incorrect fields in create form
        private bool ValidateCreateInputs(string nameImput,
            string lastNameImput,
            string mailInput,
            string docNumInput,
            string passwordInput)
        {
            bool validationOk = true;
            if (nameImput == null)
            {
                validationOk = false;
                ViewData["CreateNameImput"] = "Por favor ingrese un nombre";
            }
            else
            {
                ViewData["CreateNameImput"] = null;
            }
            if (lastNameImput == null)
            {
                validationOk = false;
                ViewData["CreateLastNameImput"] = "Por favor ingrese un apellido";
            }
            else
            {
                ViewData["CreateLastNameImput"] = null;
            }
            if (mailInput == null)
            {
                validationOk = false;
                ViewData["CreateMailInput"] = "Por favor ingrese un correo";
            }
            else if (!Regex.IsMatch(mailInput, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
            {
                validationOk = false;
                ViewData["CreateMailInput"] = "Por favor ingrese un correo valido";
            }
            else
            {
                ViewData["CreateMailInput"] = null;
            }
            if (docNumInput == null)
            {
                validationOk = false;
                ViewData["CreateDocNumInput"] = "Por favor ingrese un documento";
            }
            else
            {
                ViewData["CreateDocNumInput"] = null;
            }
            if (passwordInput == null)
            {
                validationOk = false;
                ViewData["CreatePasword"] = "Por favor ingrese un documento";
            }
            else if (passwordInput.Length < 8)
            {
                validationOk = false;
                ViewData["CreatePasword"] = "La contrasena debe tener minimo 8 caracteres";
            }
            else
            {
                ViewData["CreatePasword"] = null;
            }
            return validationOk;
        }

        // Load creation view
        public async Task<ViewResult> Create()
        {
            md = new ModelLoader();
            md.docTypeModel = await GetDocTypes();
            md.userModel = null;
            return View(md);
        }

        // Handle user creation
        [HttpPost]
        public async Task<IActionResult> Create(
            string nameImput,
            string lastNameImput,
            string mailInput,
            string docTypeModel,
            string docNumInput,
            string passwordInput
            )
        {
            md = new ModelLoader();
            md.docTypeModel = await GetDocTypes();
            if (ValidateCreateInputs(nameImput, lastNameImput, mailInput, docNumInput, passwordInput))
            {
                UserModel savedUser = new UserModel();
                savedUser.Name = nameImput;
                savedUser.DocNumber = docNumInput;
                savedUser.Password = passwordInput;
                savedUser.Email = mailInput;
                savedUser.Lastname = lastNameImput;
                savedUser.DocTypeId = Int32.Parse(docTypeModel);
                using (var httpClient = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(savedUser), Encoding.UTF8, "application/json");

                    using (var response = await httpClient.PostAsync("https://localhost:7256/api/User/", content))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            savedUser = JsonConvert.DeserializeObject<UserModel>(apiResponse);
                            md.userModel = savedUser;
                            return View(md);
                        }
                        else
                        {
                            return View(md);
                        }
                    }
                }
            }
            else
            {
                return View(md);
            }
        }

    }
}

