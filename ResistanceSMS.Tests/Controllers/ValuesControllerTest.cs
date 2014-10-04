using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResistanceSMS;
using ResistanceSMS.Controllers;

namespace ResistanceSMS.Tests.Controllers
{
	[TestClass]
	public class ValuesControllerTest
	{
		[TestMethod]
		public void Get()
		{
			// Arrange
			SMSController controller = new SMSController();

			// Act
			IEnumerable<string> result = controller.Get();

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Count());
			Assert.AreEqual("value1", result.ElementAt(0));
			Assert.AreEqual("value2", result.ElementAt(1));
		}

		[TestMethod]
		public void GetById()
		{
			// Arrange
			SMSController controller = new SMSController();

			// Act
			string result = controller.Get(5);

			// Assert
			Assert.AreEqual("value", result);
		}

		[TestMethod]
		public void Post()
		{
			// Arrange
			SMSController controller = new SMSController();

			// Act
			controller.Post("value");

			// Assert
		}

		[TestMethod]
		public void Put()
		{
			// Arrange
			SMSController controller = new SMSController();

			// Act
			controller.Put(5, "value");

			// Assert
		}

		[TestMethod]
		public void Delete()
		{
			// Arrange
			SMSController controller = new SMSController();

			// Act
			controller.Delete(5);

			// Assert
		}
	}
}
