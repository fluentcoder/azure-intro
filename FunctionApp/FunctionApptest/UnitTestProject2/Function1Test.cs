using FunctionApptest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using ExpectedObjects;
using UserCreator;

namespace UnitTestProject2
{
    [TestClass]
    public class Function1Test
    {
        [TestMethod]
        public void GetUsers_should_return_list_of_users()
        {
            //Arrange
            List<User> expectedUsers = new List<User>();

            //Act
            List<User> actualUsers = Function1.GetUsers();
            expectedUsers.Add( new User() { UserId = 1028, Name = "dsf", Age = 22 });
            expectedUsers.Add(new User() { UserId = 1029, Name = "dddddd", Age = 33333 });
            
            //Assert
            expectedUsers.ToExpectedObject().ShouldEqual(actualUsers);

            //Assert.IsTrue( expectedUsers.Where(cc => MyCompare(actualUsers, cc.UserId)).Count() == expectedUsers.Count);
        }

        //private bool MyCompare( List<User> userList, int id)
        //{
        //    var result = userList.Select(c => c.UserId == id);

        //    if (result.Count() > 0)
        //        return true;

        //    return false;
        //}
    }
}
