
// Simple bingo sheet generator for the video game "Realm of the Mad God".
// TODO: Update number that TotalNumberLabel is set to. add or subtract depending on checkboxes in checklistbox checked (In an event listener)
// TODO: Add the number of items a chelistbox option contains ex:"Untiered (248)"
// TODO: Add error more error handling when less than 25 items are added. Can freeze the program
using System.Drawing.Imaging;
using System.Reflection;

namespace ROTMG_BingoCard
{
    public partial class Form1 : Form
    {
        private PictureBox[] cardSpots;

        // Declaring array of bag image file paths
        private string[] bagArray = new string[3] { 
            "..\\..\\..\\assets\\Bags\\Orange Bag.png",
            "..\\..\\..\\assets\\Bags\\Red Bag.png", 
            "..\\..\\..\\assets\\Bags\\White Bag.png"
        };

        // Desired width and height for image export
        int bitMapWidth = 765;
        int bitMapHeight = 775;

        public Form1()
        {
            // Initializing picturboxes in order from top left most (TL_PB) to bottom right most (BR_PB)
            InitializeComponent();
            cardSpots = new PictureBox[] {TL_PB, TLM_PB, TM_PB, TRM_PB, TR_PB, MTL_PB, TMLM_PB, MTM_PB, TMTR_PB, MTR_PB, ML_PB, MLM_PB, MM_PB, MRM_PB, MR_PB,
                BLM_PB, BMLM_PB, MBM_PB, BMRM_PB, BRM_PB, BL_PB, BML_PB, BM_PB, BMR_PB, BR_PB};

        }

        private void button1_Click(object sender, EventArgs e)
        {
            randomize(cardSpots, getImageList());
        }

        // Save the bingo sheet part of the form, as a png
        // TODO: Crop out winforms borders from the exported png
        private void export_Button_Click(object sender, EventArgs e)
        {
            // Creating a bit map.
            Bitmap bingoCard = new Bitmap(bitMapWidth, bitMapHeight);
            this.DrawToBitmap(bingoCard, new Rectangle(0, 0, bitMapWidth, bitMapHeight));

            // Save bitmap as png
            SaveBitmapAsPng(bingoCard);

            bingoCard.Dispose();
        }

        // Assign a random png from a list of pngs to all image boxes 
        private void randomize(PictureBox[] boxes, string[] imgList)
        { 
            Random rnd = new Random();

            // Tracking already rolled images, so no duplicates are added. AND tracking boxes index rolled for bag placement
            List<int> imgRolled = new List<int>();
            List<int> boxRolled = new List<int>();
 
            int imgIndex = 0;
            int boxIndex = 0;

            // Clear the already rolled index values
            imgRolled.Clear();
            boxRolled.Clear();

            if (imgList.Length != 0)
            {
                // For each Picture box assign it an image from an array of images
                foreach (PictureBox box in boxes)
                {
                    bool repeat = true;
                    while (repeat)
                    {
                        // Generate random index
                        imgIndex = rnd.Next(imgList.Length);

                        bool isDuplicate = false;
                        // loop through already rolled numbers to check for duplicates
                        foreach (int i in imgRolled)
                        {
                            if (imgIndex == i) { isDuplicate = true; break; }
                        }

                        repeat = isDuplicate;
                    }
                    // Add roll to existing rolls after searching for duplicates
                    imgRolled.Add(imgIndex);

                    resizeDisplay(box, imgList[imgIndex]);
                }

                // If "Bag it Up!" is selected
                if (goalsCheckListBox.GetItemChecked(4))
                {
                    // Adding a ST bag, white bag and red bag on a random space
                    foreach (string bag in bagArray)
                    {
                        bool repeat = true;
                        while (repeat)
                        {
                            // Generate random index
                            boxIndex = rnd.Next(boxes.Length);

                            bool isDuplicate = false;
                            // loop through already rolled numbers to check for duplicates
                            foreach (int i in boxRolled)
                            {
                                if (boxIndex == i) { isDuplicate = true; break; }
                            }

                            repeat = isDuplicate;
                        }
                        boxRolled.Add(boxIndex);

                        resizeDisplay(boxes[boxIndex], bag);
                    }
                }

                // If "Shiny Any" is selected. Add to a non-"bag it up" space or middle
                if (goalsCheckListBox.GetItemChecked(6))
                {
                        bool repeat = true;
                        while (repeat)
                        {
                            // Generate random index
                            boxIndex = rnd.Next(boxes.Length);

                            bool isDuplicate = false;
                            // loop through already rolled numbers to check for duplicates
                            foreach (int i in boxRolled)
                            {
                                if (boxIndex == i) { isDuplicate = true; break; }
                                else if (boxIndex == 12) { isDuplicate = true; break; }
                            }

                            repeat = isDuplicate;
                        }
                        boxRolled.Add(boxIndex);

                        resizeDisplay(boxes[boxIndex], "..\\..\\..\\assets\\Shiny Any\\Any Shiny.png");
                }

                // Free white bag in middle
                if (goalsCheckListBox.GetItemChecked(3))
                {
                    resizeDisplay(MM_PB, bagArray[2]);
                }
            }
        }

        // Function to resize an image of a picture box to fit and display it.
        private void resizeDisplay(PictureBox box, string imgPath)
        {
            box.SizeMode = PictureBoxSizeMode.StretchImage;
            box.Image = Image.FromFile(imgPath);
        }

        // Function to create a save file dialog
        private void SaveBitmapAsPng(Bitmap bitmap)
        {
            using(SaveFileDialog saveDialog = new SaveFileDialog())
            {
                // filter shows pngs
                saveDialog.Filter = "PNG Files (*.png)|*.png";

                // Show the SaveFileDialog
                if(saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // File chosen by user
                    string filePath = saveDialog.FileName;

                    // Save bitmap as png to file
                    bitmap.Save(filePath, ImageFormat.Png);
                }
            }
        }

        // Function to retrieve array of images to get from file based on checked boxes
        private string[] getImageList()
        {
            //// @ NOTE: Will RETRIEVE ALL files from selected directories on EACH "Start" buton push. This is inefficient
            ////        Should be keeping track of currently selected and ONLY RETRIEVING files if something changes.
            ////        Is a very small & simple project that doesn't necessite optimization.
            ////        Another method would be to load a sprite map(a png with all sprites on it) and create a bitmap to section off each sprite.

            // Base directory path
            string baseDirPath = "..\\..\\..\\assets\\";

            // List of subdirectories within the base path
            List<string> subDirList = new List<string>();

            // List of img pngs
            List<string> combinedList = new List<string>();

            // Total number of elements in selected items
            int totalSelectedLength = 0;

            // Only get sub-directories and add them if the combined total of items in subdirectories is 25 or greater. 25 slots in a bingo card
            if (goalsCheckListBox.CheckedItems.Count != 0)
            {
                // Loop through checked items collection
                for (int ii = 0; ii < goalsCheckListBox.CheckedItems.Count; ii++)
                {
                    // Check which boxes are checked and add their filenames to our list of subdirectories
                    switch (goalsCheckListBox.CheckedItems[ii].ToString())
                    {
                        case "Untiered":
                            subDirList.Add("UT");
                            break;
                        case "Set Tiered":
                            subDirList.Add("ST"); 
                            break;
                        case "Tiered":
                            subDirList.Add("Tiered");
                            break;
                        case "Any of a Tier":
                            subDirList.Add("Tiered Any");
                            break;
                        case "Shiny":
                            subDirList.Add("Shiny");
                            break;
                        // Cases to account for options that are dealt with in different function scope who's number is not added to combinedList Count's 
                        case "Any Shiny":
                            totalSelectedLength++;
                            break;
                        case "Bag it Up":
                            totalSelectedLength += 3;
                            break;
                        case "Free Middle":
                            totalSelectedLength++;
                            break;
                    }
                } 

                // Adding list of png files together using a temp list & AddRange()
                foreach(string subDir in subDirList)
                {
                    string[] tempArray = Directory.GetFiles(baseDirPath + subDir, "*.png");

                    combinedList.AddRange(tempArray);
                }
                
                totalSelectedLength += combinedList.Count;

                // Message out that the user needs to select more items
                if (combinedList.Count < 25)
                {
                    MessageBox.Show("Please check off items until at least 25 items can be added... Current items: " + totalSelectedLength);
                }
            }
            else
            {
                MessageBox.Show("Please check off items until at least 25 items can be added... Current items: 0");
            }

            string[] combined = combinedList.ToArray();

            return combined;
        }
    }
}