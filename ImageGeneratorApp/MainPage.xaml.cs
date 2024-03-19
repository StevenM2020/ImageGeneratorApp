//using System.Text;
//using System.Xml;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Storage = ImageGeneratorApp.Storage;
using Validation = ImageGeneratorApp.Validation;
using static System.Net.Mime.MediaTypeNames;


namespace ImageGeneratorApp
{

    public partial class MainPage : ContentPage
    {
        int count = 0;
        bool LoginScreen = true;

        public MainPage()
        {
            InitializeComponent();
        }

        // when the user clicks the button or presses enter
        private async void OnActionButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Home());
            Navigation.RemovePage(this);
            return;
            if (LoginScreen)
            {
                Login();
            }
            else
            {
                SignUp();
            }
        }

        // this function checks if the email and password are valid and then logs the user in
        private async void Login()
        {
            string email = txtEmail.Text;
            string password = txtPassword.Text;

            // check if the email and password are valid
            //if (!(Validation.ValidateEmail(email) && Validation.ValidatePassword(password)))
            //{
            //    DisplayAlert("Error", "Email or password does not meet requirements", "OK");
            //    return;
            //}

            // check if the email and password are correct
            bool blnCheckHash = await Storage.CheckPassword( email, password);
            if (!blnCheckHash)
            {
                DisplayAlert("Error", "Invalid email or password", "OK");
                return;
            }

            // save the user's ID to the secure storage
            Storage.SaveUserID(email);
            DisplayAlert("", "It worked!", "OK");
        }



        // this function checks if the email and password are valid and then signs the user up
        private async void SignUp()
        {
            string email = txtEmail.Text;
            string password = txtPassword.Text;


            if (!Validation.ValidateAge(birthdayPicker))
            {
                DisplayAlert("Error", "Users must be 18 years or older.", "OK");
                return;
            }


            if (!Validation.ValidateEmail(txtEmail) || !Validation.ValidatePassword(txtPassword))
            {
                //DisplayAlert("Error", "Invalid email or password", "OK");
                return;
            }

            //check if the email already exists
            bool blnCheckUser = await Storage.CheckUserExists(email);
            if (blnCheckUser)
            {
                //DisplayAlert("Error", "Invalid email", "OK");
                txtEmail.BackgroundColor = Color.FromRgb(100, 0, 0);
                return;
            }else
                txtEmail.BackgroundColor = Color.FromRgb(31, 31, 31);

            Storage.CreateUser(email, password);
            Storage.SaveUserID(email);
            await Navigation.PushAsync(new Home());
        }

        // this function is called when the user types in the email entry
        private void OnTextChanged(object sender, EventArgs e)
        {
            Entry entry = (Entry)sender;
            entry.Text = entry.Text.Trim();

        }


        // this function is called when the user taps the switch to change the login screen or sign up screen
        private void OnSwitchTapped(object sender, EventArgs e)
        {

            LoginScreen = !LoginScreen;
            if (LoginScreen)
            {
                btnAction.Text = "Login";
                lblSwitch.Text = "Sign up";
                lblSwitchQuestion.Text = "Don't have an account?";
                lblWelcome.Text = "Welcome, login to start";
                stkDate.IsVisible = false;
            }
            else
            {
                btnAction.Text = "Sign Up";
                lblSwitch.Text = "Login";
                lblSwitchQuestion.Text = "Already have an account?";
                lblWelcome.Text = "Welcome, make an account to start";
                stkDate.IsVisible = true;
            }
        }
    }
}
