using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AADB2C.JITUserMigration.Models
{
    public class UserTableEntity: TableEntity
    {
        public UserTableEntity()
        {

        }

        public UserTableEntity(string signInName, string password, string firstName, string lastName)
        {
            this.PartitionKey = Consts.MigrationTablePartition;
            this.RowKey = signInName.ToLower();
            this.Password = password;
            this.DisplayName = firstName + " " +  lastName;
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
