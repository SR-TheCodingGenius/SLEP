using SLEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SLEP.Extensions.Handlers
{
	public static class PopulateSession
	{
		private static object PopulateSessionList(this DataGrid dataGrid,object model)
		{
			var sessCollection = dataGrid.Items.SourceCollection;
			var sessionList = (sessCollection.Cast<ABCSessionModel>().Select(sess => sess)).ToList();

			dynamic modelList = (ABCSessionModel)model;

			if(modelList == null)
			{
				modelList = (MushraSessionModel)model;
			}
			
			foreach (var obj in sessionList)
			{
				modelList.trials.Add(obj);
			}
			return modelList;
		}
	}
}
