using SLEP.Models;
using SLEP.UIModule.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace SLEP.UIModule.Views
{
    /// <summary>
    /// Interaction logic for CustomScales.xaml
    /// </summary>
    public partial class CustomScales : UserControl
    {
        private CustomScaleProperties _customScaleProperties;
        private IList<Label> _customLabels = new List<Label>();
        private IList<TextBox> _customComments = new List<TextBox>();
        private IList<CustomScaleProperties> _customScalePropertiesList = new List<CustomScaleProperties>();
        private IList<RadioButton> _Customradios=new List<RadioButton>();
        public CustomScales()
        {
            InitializeComponent();

            _customLabels.Add(CommentL1);
            _customLabels.Add(CommentL2);
            _customLabels.Add(CommentL3);
            _customLabels.Add(CommentL4);
            _customLabels.Add(CommentL5);
            _customLabels.Add(CommentL6);
            _customLabels.Add(CommentL7);

            _customComments.Add(CommentTB1);
            _customComments.Add(CommentTB2);
            _customComments.Add(CommentTB3);
            _customComments.Add(CommentTB4);
            _customComments.Add(CommentTB5);
            _customComments.Add(CommentTB6);
            _customComments.Add(CommentTB7);

            _Customradios.Add(Range1);
            _Customradios.Add(Range2);
            _Customradios.Add(Range3);

            ReadCustomScaleElements();
        }

        private void OnRangeSelectionChanged(object sender, RoutedEventArgs e)
        {
            var radioBtn = e.OriginalSource as RadioButton;
            _customComments.ToList().ForEach(textBox => textBox.IsEnabled = true);
            if (radioBtn.Name == "Range1")
            {
                var count = 5;

                _customLabels.ToList().ForEach(item =>
                {
                    if (count > 0)
                    {
                        item.Content = count.ToString();
                    }
                    else
                    {
                        item.Content = "";
                        CommentTB6.IsEnabled = false;
                        CommentTB7.IsEnabled = false;
                    }
                    count -= 1;
                });
            }
            else if (radioBtn.Name == "Range2")
            {
                var count = 3;
                _customLabels.ToList().ForEach(item =>
                {
                    item.Content = count.ToString();
                    count -= 1;
                });
                CommentTB7.IsEnabled = true;
                CommentTB6.IsEnabled = true;
            }
            else if (radioBtn.Name == "Range3")
            {
                var count = 100;
                _customLabels.ToList().ForEach(item =>
                {
                    if (count >= 0)
                    {
                        item.Content = count.ToString();
                    }
                    else
                    {
                        item.Content = "";
                        CommentTB7.IsEnabled = false;
                        CommentTB6.IsEnabled = true;
                    }
                    count -= 20;
                });
            }

        }

        private void OnClearAll(object sender, RoutedEventArgs e)
        {
            ClearFields();
            ScaleTBox.Text = "";
            _customComments.ToList().ForEach(textBox => textBox.IsEnabled = false);
        }

        private bool ValidateScaleProperties(int item)
        {
            bool radiocheck=false;
            bool flag = true;
            var scaleName = ScaleTBox.Text;
            scaleName = scaleName.Trim(); ;
            var regex = new Regex(@"[^a-zA-Z0-9\s]");

            if (ScaleTBox.Text == "")
            {
                MessageBox.Show("All Editable Fields are Mandatory!!!!\n They can't have blank or duplicates", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return false;
            }

            if (Regex.IsMatch(scaleName[0].ToString(), "[0-9]"))
            {
                MessageBox.Show("Scale name should not starts with integer!!!!!!!!", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (regex.IsMatch(scaleName.ToString()))
            {
                MessageBox.Show("Special characters not allowed!!!!!!!!", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                ScaleTBox.Text = "";
                return false;
            }

            if (item == 1)
            {
                if (ScaleLBox.Items.Contains(scaleName))
                {
                    MessageBox.Show("Scale Name should be unique!!!!!!!!", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }

                foreach (var v in _Customradios)
                {
                    if (v.IsChecked == true)
                    {
                        radiocheck = true;
                        break;
                    }
                }

                if (radiocheck==false)
                {
                    MessageBox.Show("Range is Mandatory!!!!!!!!", "Info", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return false;
                }
               
                else
                {
                    flag = _customComments.Where(textBox => textBox.IsEnabled == true).All(comment =>
                    {
                        if (comment.Text != "")
                        {
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("All Editable Fields are Mandatory!!!!\n They can't have blank or duplicates", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            return false;
                        }

                    });
                }
            }

            if (item == 2)
            {

                if (!ScaleLBox.Items.Contains(scaleName))
                {
                   // MessageBox.Show("Scale Name !!!!!!!!", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }

                flag = _customComments.Where(textBox => textBox.IsEnabled == true).All(comment =>
						{
							if (comment.Text != "")
							{
								return true;
							}
							else
							{
								MessageBox.Show("All Editable Fields are Mandatory!!!!\n They can't have blank or duplicates",
									"Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
								return false;
							}

						});
            }

            var enabledTextBoxesCount = _customComments.Where(textBox => textBox.IsEnabled == true).Count();
            if (flag)
            {
                for (int elem1 = 0; elem1 < enabledTextBoxesCount; elem1++)
                {
                    for (int restOfThem = elem1 + 1; restOfThem < enabledTextBoxesCount; restOfThem++)
                    {
                        if (_customComments[elem1].Text == _customComments[restOfThem].Text)
                        {
                            _customComments[restOfThem].Text = "";
                            MessageBox.Show(" Fields can't have duplicates", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            return false;
                        }
                    }
                }
            }

            return flag;
        }
        bool _addToListClicked = false;
        private void OnAddListClicked(object sender, RoutedEventArgs e)
        {
            //if (ValidateScaleProperties() == false)
            //{
            //	MessageBox.Show("All Editable Fields are Mandatory!!!!\n They can't be left blank", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            //	return;
            //}

            //FillDataFromControlsToBuffers();

            //var scaleName = _customScaleProperties.ScaleName;
            //ScaleLBox.Items.Add(scaleName);
            //_addToListClicked = true;

            if (ValidateScaleProperties(1))
            {
                FillDataFromControlsToBuffers();
                var scaleName = _customScaleProperties.ScaleName;
                ScaleLBox.Items.Add(scaleName);
                ClearFields();
				_addToListClicked = true;
			}
        }

        private void FillDataFromControlsToBuffers()
        {
            _customScaleProperties = new CustomScaleProperties();
            _customScaleProperties.ScaleName = ScaleTBox.Text;

            // Adding the TextBox content to the list
            _customComments.Where(txtBox => txtBox.IsEnabled == true).ToList().ForEach(textBox =>
            {
                _customScaleProperties.CustomComments.Add(textBox.Text);
            });

            // Adding the Label content to the list
            _customLabels.Where(label => label.Content.ToString() != "").ToList().ForEach(label =>
            {
                _customScaleProperties.CustomScaleLabels.Add(label.Content.ToString());
            });

            _customScalePropertiesList.Add(_customScaleProperties);
        }

        private void StoreCustomScaleConfiguration(IList<CustomScaleProperties> scaleList)
        {
            using (var stream = new FileStream("CustomScale.config", FileMode.Append))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                var xmlWriter = XmlWriter.Create(stream, settings);

                scaleList.ToList().ForEach(scale =>
                {
                    xmlWriter.WriteStartElement(scale.ScaleName);

                    for (var count = scale.CustomScaleLabels.Count - 1; count >= 0; count--)
                    {
                        xmlWriter.WriteStartElement("add");
                        xmlWriter.WriteAttributeString("key", scale.CustomScaleLabels[count]);
                        xmlWriter.WriteAttributeString("value", scale.CustomComments[count]);
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                });

                xmlWriter.Flush();
                xmlWriter.Close();
                stream.Close();
            }


        }

        private void OnOKBtnClicked(object sender, RoutedEventArgs e)
        {
            if (_addToListClicked)
            {
                if (_customScalePropertiesList.Count > 0)
                {
                    StoreCustomScaleConfiguration(_customScalePropertiesList);
                    MessageBox.Show("The Custom Scales added to the list are saved to file", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearFields();
                }
            }

            var parentWindow = (Window)this.Parent;
            parentWindow.DialogResult = true;
            parentWindow.Close();
        }

        private void OnModifyClicked(object sender, RoutedEventArgs e)
        {

            var customScaleProperties = new CustomScaleProperties();
            if (ScaleTBox.Text != "")
            {
                if (ValidateScaleProperties(2))
                {

                    customScaleProperties.ScaleName = ScaleTBox.Text;
                    _customLabels.ToList().ForEach(label =>
                    {
                        if (label.Content.ToString() != "")
                        {
                            customScaleProperties.CustomScaleLabels.Add(label.Content.ToString());
                        }
                    });

                    _customComments.ToList().ForEach(textbox =>
                    {
                        if (textbox.Text != "")
                        {
                            customScaleProperties.CustomComments.Add(textbox.Text);
                        }
                    });

                    var scaleList = new List<CustomScaleProperties> { customScaleProperties };
                    RemoveSelectedScale();
                    StoreCustomScaleConfiguration(scaleList);
                    ScaleLBox.Items.Add(customScaleProperties.ScaleName);                    
                    MessageBox.Show("The Scale has been modified.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearFields();
                }
            }
            else
            {
                MessageBox.Show("No Scale has been selected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnRemoveClicked(object sender, RoutedEventArgs e)
        {
            if (ScaleTBox.Text == "")
            {
                MessageBox.Show("No Scale has been selected", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (MessageBox.Show("Are you sure , you want to delete the scale.", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {

                RemoveSelectedScale();
                ClearFields();
                Range1.IsChecked = false;
                Range2.IsChecked = false;
                Range3.IsChecked = false;
            }
        }

        private void ClearFields()
        {
            ScaleTBox.Text = "";
            _addToListClicked = false;
            _customComments.ToList().ForEach(item => item.Clear());
            _customLabels.ToList().ForEach(item => item.Content = "");
            _addToListClicked = false;
            Range1.IsChecked = false;
            Range2.IsChecked = false;
            Range3.IsChecked = false;
        }

        private void RemoveSelectedScale()
        {
            var fileName = "CustomScale.config";
            var xmlContent = File.ReadAllText(fileName);
            var xmlDoc = XDocument.Parse("<root>" + xmlContent + "</root>");

            xmlDoc.Descendants(ScaleTBox.Text)
            .Where(node => node.Name == ScaleTBox.Text)
            .Remove();
            var newXml = xmlDoc.ToString();

            newXml = newXml.Replace("<root>", "");
            newXml = newXml.Replace("</root>", "");
            newXml = newXml.Replace("<root />", "");
            File.WriteAllText(fileName, newXml);
            ScaleLBox.Items.Remove(ScaleTBox.Text);
        }
        private void OnScaleSelected(object sender, SelectionChangedEventArgs e)
        {
            ModifyBtn.IsEnabled = true;
            _customComments.ToList().ForEach(item => item.Clear());
            _customLabels.ToList().ForEach(item => item.Content = "");
            bool readFlag;
            var selectedItem = ScaleLBox.SelectedItem?.ToString();
            ScaleTBox.Text = selectedItem;

            // Searches the scale in config file, if found retireves it and displays
            using (var customScale = new CustomScale(selectedItem))
            {
                if (customScale == null)
                {
                    return;
                }
                readFlag = customScale.ReadCustomScaleConfigFile();
                if (readFlag)
                {
                    var count = 0;
                    customScale.Score.Reverse().ToList().ForEach(item =>
                    {
                        _customLabels[count].Content = item;
                        count++;
                    });


                    count = 0;
                    customScale.ScoreComment.Reverse().ToList().ForEach(item =>
                    {
                        _customComments[count].Text = item;
                        count++;
                    });
                }
            }

            // If scale is not found, it retrieves from in-memory
            if (readFlag == false)
            {
                _customScalePropertiesList.ToList().ForEach(item =>
                {
                    var count = 0;
                    if (item.ScaleName == selectedItem)
                    {
                        item.CustomComments.ToList().ForEach(comment =>
                        {
                            _customComments[count].Text = comment;
                            count++;
                        });
                        count = 0;
                        item.CustomScaleLabels.ToList().ForEach(labels =>
                        {
                            _customLabels[count].Content = labels;
                            count++;
                        });
                    }
                });

            }


            SetRangeButtons();

            _customComments.ToList().ForEach(item =>
            {
                if (item.Text == "")
                {
                    item.IsEnabled = false;
                }
                else
                {
                    item.IsEnabled = true;
                }
            });
        }

        private void SetRangeButtons()
        {
            var firstScaleElement = _customLabels.FirstOrDefault().Content.ToString();
            switch (firstScaleElement)
            {
                case "5":
                    Range1.IsChecked = true;
                    Range2.IsChecked = false;
                    Range3.IsChecked = false;
                    break;
                case "3":
                    Range1.IsChecked = false;
                    Range2.IsChecked = true;
                    Range3.IsChecked = false;
                    break;
                case "100":
                    Range1.IsChecked = false;
                    Range2.IsChecked = false;
                    Range3.IsChecked = true;
                    break;

            }
        }
        private void ReadCustomScaleElements()
        {
            var fileName = "CustomScale.config";
            if (File.Exists(fileName))
            {
                var settings = new XmlReaderSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                using (var reader = XmlReader.Create(fileName, settings))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            ScaleLBox.Items.Add(reader.Name);
                        }
                    }
                }
            }
        }

    }


}
