using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SLEP.UIModule.Handlers
{
	public class CustomScale : IScoringScales, IDisposable
	{
		private string _customScaleName = "";
		private IList<string> _score = new List<string>();
		private IList<string> _scoreComment = new List<string>();
		private int _scaleLength = 0;
		public CustomScale(string scaleName)
		{
			_customScaleName = scaleName;
		}
		
		public IList<string> Score { get { return _score; } }
		public IList<string> ScoreComment { get { return _scoreComment; } }
		public NameValueCollection LoadScale()
		{
			if (ReadCustomScaleConfigFile() == false)
			{
				return null;
			}
			else
			{ 
				var nameValueCollection = new NameValueCollection();

				for (int count = 0; count < _scaleLength; count++)
				{
					nameValueCollection.Add(_scoreComment[count], _score[count]);
				}
				return nameValueCollection;
			}
			
		}
		public bool ReadCustomScaleConfigFile()
		{
			var fileName = "CustomScale.config";
			bool flag = false;
			_score.Clear();
			_scoreComment.Clear();
			if (File.Exists(fileName))
			{
				var settings = new XmlReaderSettings();
				settings.ConformanceLevel = ConformanceLevel.Fragment;
				
				using (var reader = XmlReader.Create(fileName, settings))
				{
					while (reader.Read())
					{
						if(_customScaleName == null)
						{
							return false;
						}
						
							if (reader.Name.ToLower() == (_customScaleName.ToLower().Trim()))
							{
								while (reader.Read())
								{
									if (reader.NodeType != XmlNodeType.EndElement && reader.NodeType == XmlNodeType.Element)
									{
										
										_score.Add(reader.GetAttribute("key"));
										_scoreComment.Add(reader.GetAttribute("value"));
										_scaleLength++;
									}
									if(reader.NodeType == XmlNodeType.EndElement)
									{
										if(reader.Name.ToLower() == (_customScaleName.ToLower().Trim()))
										{
											flag = true;
											break;
										}
									}
								}

								if (flag)
								{
									break;
								}
							}
							//else
							//{
							//	flag = false;
							//}
							
					}

				}
			}
			return flag;
		}// end of method

		public void Dispose()
		{
			
		}
	}
}
