// Program:     Page for generating images
// Author:      Steven Motz
// Date:        03/18/2024
// Description: This is the page for generating images. It allows the user to generate images based on a prompt and view the images they have generated.
using System.Diagnostics;
using ImageGen = ImageGeneratorApp.ImageGeneration;
using Validation = ImageGeneratorApp.Validation;
using Storage = ImageGeneratorApp.Storage;
using System.Collections.ObjectModel;

namespace ImageGeneratorApp;
public partial class Home : ContentPage
{
	public Home()
	{
		InitializeComponent();
        LoadImages();

    }

    // when the slider value changes, round the value and set the label to the rounded value
    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        int roundedValue = (int)Math.Round(e.NewValue);

        // Set the slider to the rounded value
        ((Slider)sender).Value = roundedValue;

        // Set the label to the rounded value
        lblImageSize.Text = ImageGen.GetImageSize(roundedValue).ToString();
    }

    // when the user clicks the generate image button it will generate an image based on the prompt
    private async void OnActionButtonClicked(object sender, EventArgs e)
    {
        try
        {

            btnGenerateImage.IsEnabled = false;
            btnGenerateImage.Text = "Testing...";
            if (Validation.ValidatePrompt(txtPrompt))
            {
                bool blnPrompt = await ImageGen.ValidatePrompt(txtPrompt.Text);
                if (blnPrompt)
                {
                    string ImprovedPrompt = await ImageGen.ImprovePrompt(txtPrompt.Text);
                    lblImprovedPrompt.Text = ImprovedPrompt;
                    stkImprovedPrompt.IsVisible = true;
                    btnGenerateImage.IsEnabled = false;
                    btnGenerateImage.Text = "Generating...";
                    string base64Image =
                        await ImageGen.GenerateImage(ImprovedPrompt,
                            (int)Math.Round(sldImageSize.Value));                // generate the image
                    imgGeneratedImage.Source = ImageGen.Base64StringToImage(base64Image); // display the image
                    Storage.SaveImage(base64Image);                                       // save the image
                    AddImageToCollection(base64Image);                                    // add the image to the collection
                }
                else
                {
                    DisplayAlert("Error", "Invalid prompt", "OK");
                }
            }
            else
            {
                DisplayAlert("Error", "Invalid prompt", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            DisplayAlert("Error", "Failed to generate image", "OK");
        }
        btnGenerateImage.IsEnabled = true;
        btnGenerateImage.Text = "Generate Image";
    }

    // loads the images from the database and displays them in the collection
    private async Task LoadImages()
    {
        try
        {
            var imageStrings = await Storage.GetImages();
            List<ImageSource> imageSources = new List<ImageSource>();
            foreach (var imgStr in imageStrings)
            {
                var imgSource = ImageGen.Base64StringToImage(imgStr);
                imageSources.Add(imgSource);
            }

            imagesCollection.ItemsSource = imageSources;
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", "Failed to load images", "OK");
        }
    }

    // adds the image to the collection
    private void AddImageToCollection(string base64Image)
    {
        List<ImageSource> imageSources = new List<ImageSource>();
        imageSources = (List<ImageSource>)imagesCollection.ItemsSource;
        imageSources.Add(ImageGen.Base64StringToImage(base64Image));
        imagesCollection.ItemsSource = imageSources;
    }

    // when the user clicks on an image it will be displayed in the larger image view and scrolled to it
    private void OnImageClicked(object sender, EventArgs e)
    {
        imgGeneratedImage.Source = ((Image)sender).Source;
        scrollContainer.ScrollToAsync(0, imgGeneratedImage.Y - 10, true);
    }

    // when the user clicks the logout button it will log the user out and take them back to the login page
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
        Navigation.RemovePage(this);
    }
}
