using System.Diagnostics;
using ImageGen = ImageGeneratorApp.ImageGeneration;
using Validation = ImageGeneratorApp.Validation;
using Storage = ImageGeneratorApp.Storage;
using static System.Windows.Forms.ImageList;
using System.Collections.ObjectModel;

namespace ImageGeneratorApp;
public partial class Home : ContentPage
{
	public Home()
	{
		InitializeComponent();
        LoadImages();

    }

    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        int roundedValue = (int)Math.Round(e.NewValue);

        // Set the slider to the rounded value
        ((Slider)sender).Value = roundedValue;

        // Set the label to the rounded value
        lblImageSize.Text = ImageGen.GetImageSize(roundedValue).ToString();
    }

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
    
    }