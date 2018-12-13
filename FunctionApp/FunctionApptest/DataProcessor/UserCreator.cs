using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserCreator
{
    public class UserCreator
    {
        public User CreateUser(int id, string name, int age)
        {
            return new User(){Age=age,UserId=id,Name=name};
        }
    }
}
