using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DataBase
{
    public  class SqlHelper
    {
        private OleDbHelper oledb;
        private string sqlPath;
        private string dataBaseName = "Hello.accdb";
        private string provider;
        public SqlHelper()
        {
            oledb = new OleDbHelper();
            oledb.Url = AppSetting.Get("ConnectionString");

        }

        /// <summary>
        /// 测试连接
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool TestConn
        {
            get
            {
                return oledb.OleDbConnectionTest(oledb.Url);
            }
        }

        public void CreateTable()
        {
            string commandText = @"CREATE TABLE {0}({1} CHAR(10) NOT NULL,{2} 类型(数据长度))";
            var conn = new OleDbConnection(oledb.Url);
            //
        }

        public DataTable GetDataTable(string commandText)
        {
            DataSet ds;
            oledb.GetDataSet(commandText, out ds);
            return ds.Tables.Count == 0 ? null : ds.Tables[0];
        }
        public object ExecuteScalar(string commandText)
        {
            return oledb.GetSingle(commandText);
        }
        public bool Execute(string commandText)
        {
            return oledb.OleDbExecute(commandText);
        }

        public bool Execute(string commandText, OleDbParameter[] paras)
        {
            return oledb.OleDbExecute(commandText, paras);
        }

        public bool Execute(string commandText, DataTable dt)
        {
            return oledb.UpdateDbData(commandText, dt);
        }
    }

    /*
    增删改查例子
    增：insert into table (字段1, 字段2) values ('值','值')

    删：delete from table where 字段 = '值'

    改：update table set 姓名='梁朝伟' , 年龄='55' where 姓名= '梁朝伟'"

    查：select * from table where 字段 = '值'
    */
}

