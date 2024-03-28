// Class:       Validation
// Author:      Steven Motz
// Date:        03/18/2024
// Description: This class contains functions for validating user input. It contains functions for validating email, password, age, and prompt.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace ImageGeneratorApp
{
    internal static class Validation
    {
        // checks if the email is a valid email address and changes the background color of the entry to red if it is not
        internal static bool ValidateEmail(object sender)
        {
            Debug.WriteLine("test email");
            Entry email = (Entry)sender;
            string strEmail = email.Text;
            if (email.Text == null)
            {
                email.BackgroundColor = Color.FromRgb(100, 0, 0);
                return false;
            }

            var emailAddressAttribute = new EmailAddressAttribute();

            if (!emailAddressAttribute.IsValid(email.Text)) 
            {
                email.BackgroundColor = Color.FromRgb(100, 0, 0);
                Console.WriteLine("Invalid email");
                return false;
            }
            email.BackgroundColor = Color.FromRgb(31, 31, 31);
            return true;
        }

        // checks if the password is a valid password and changes the background color of the entry to red if it is not
        // a valid password must be at least 8 char long and can contain letters, numbers, and special characters
        internal static bool ValidatePassword(object sender)
        {
            Entry password = (Entry)sender;
            string strEmail = password.Text;
            if (password.Text == null)
            {
                password.BackgroundColor = Color.FromRgb(100, 0, 0);
                return false;
            }
            if (!Regex.IsMatch(strEmail, @"^[a-zA-Z0-9@#$!%^&*]{8,}$"))
            {
                password.BackgroundColor = Color.FromRgb(100, 0, 0);
                return false;
            }
            password.BackgroundColor = Color.FromRgb(31, 31, 31);
            return true;
        }
        //https://regex101.com/

        // checks if the user is at least 18 years old
        internal static bool ValidateAge(DatePicker date)
        {
            if (date.Date == null)
            {
                date.BackgroundColor = Color.FromRgb(100, 0, 0);
                return false;
            }
            DateTime today = DateTime.Today;
            int age = today.Year - date.Date.Year;
            if (age < 18)
            {
                date.BackgroundColor = Color.FromRgb(100, 0, 0);
                return false;
            }
            date.BackgroundColor = Color.FromRgb(31, 31, 31);
            return true;
        }

        // checks if the prompt is valid and changes the background color of the entry to red if it is not
        internal static bool ValidatePrompt(object sender)
        {
            Entry prompt = (Entry)sender;
            string strPrompt = prompt.Text;
            if (prompt.Text == null)
            {
                prompt.BackgroundColor = Color.FromRgb(100, 0, 0);
                return false;
            }

            if (strPrompt.Trim().Length <= 0)
            {
                prompt.BackgroundColor = Color.FromRgb(100, 0, 0);
                return false;
            }

            if (!Regex.IsMatch(strPrompt, @"^[a-zA-Z0-9.!? ]{3,}$"))
            {
                prompt.BackgroundColor = Color.FromRgb(100, 0, 0);
                return false;
            }
            
            prompt.BackgroundColor = Color.FromRgb(31, 31, 31);
            return true;
        }
    }
}
