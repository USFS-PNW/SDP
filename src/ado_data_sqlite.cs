using System;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
namespace SQLite.ADO
{
	/// <summary>
	/// Summary description for SQLite.
    /// Install supporting libraries using nuget System.Data.SQLite.Core
	/// </summary>
    
	public class DataMgr
	{
		public System.Data.DataSet m_DataSet;
		public System.Data.SQLite.SQLiteDataAdapter m_DataAdapter;
        public System.Data.SQLite.SQLiteDataAdapter[] m_DataAdapterArray;
        public string[] m_DataAdapterArrayTableName;
		public System.Data.SQLite.SQLiteCommand m_Command;
		public System.Data.SQLite.SQLiteConnection m_Connection;
		public System.Data.SQLite.SQLiteDataReader m_DataReader;
		public System.Data.DataTable m_DataTable;
		public System.Data.SQLite.SQLiteTransaction m_Transaction;
		public string m_strError;
		public int m_intError;
		public string m_strSQL;
		public string m_strTable;
		private bool _bDisplayErrors=true;
		private string _strMsgBoxTitle="NOMS";

        private int _intDataAdapterTableCount = 0;
        public int DataAdapterTableCount
        {
            get { return _intDataAdapterTableCount; }
            set { _intDataAdapterTableCount = value; }
        }
        private bool _bConnectionDisposed = true;
        public bool ConnectionDisposed
        {
            get { return _bConnectionDisposed; }
            set { _bConnectionDisposed = value; }
        }

		public DataMgr()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		~DataMgr()
		{

		}
        public void CreateDbFile(string p_strFile)
        {
            this.m_intError = 0;
            this.m_strError = "";
            if (System.IO.File.Exists(p_strFile)==false)
                SQLiteConnection.CreateFile(p_strFile);
        }
		public void OpenConnection(string strConn)
		{
            m_intError = 0;
            m_strError = "";
			System.Data.SQLite.SQLiteConnection p_Connection = new System.Data.SQLite.SQLiteConnection();
			p_Connection.ConnectionString = strConn;
            try
            {
                p_Connection.Open();

            }
            catch (Exception caught)
            {
                this.m_strError = caught.Message;
                if (m_strError.IndexOf("ORA-03134", 0) >= 0 ||
                    m_strError.IndexOf("ORA-28273", 0) >= 0) m_intError = -2;
                else this.m_intError = -1;
                if (_bDisplayErrors)
                    MessageBox.Show("!!Error!! \n" +
                        "Module - SQLite:OpenConnection  \n" +
                        "Err Msg - " + this.m_strError,
                        "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Exclamation);
                return;
            }
            finally
            {
                if (m_intError == 0)
                {
                    this.m_Connection = p_Connection;
                    _bConnectionDisposed = false;
                    p_Connection.Disposed += new EventHandler(this.DisposedEvent);
                }
            }
			

		}
		public void OpenConnection(string strConn, ref System.Data.SQLite.SQLiteConnection p_Connection)
		{
            this.m_intError=0;
			this.m_strError="";
            try
            {
                p_Connection.ConnectionString = strConn;
                _bConnectionDisposed = false;
                p_Connection.Open();

            }
            catch (Exception caught)
            {
                this.m_strError = caught.Message;
                this.m_intError = -1;
                if (_bDisplayErrors)
                    MessageBox.Show("!!Error!! \n" +
                        "Module - SQLite:OpenConnection  \n" +
                        "Err Msg - " + this.m_strError,
                        "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Exclamation);
            }
            finally
            {
                if (m_intError == 0)
                {
                    _bConnectionDisposed = false;
                    p_Connection.Disposed += new EventHandler(this.DisposedEvent);
                }
            }
		}

		public void SqlNonQuery(string strConn, string strSQL)
		{
            this.m_intError = 0;
            this.m_strError = "";
            System.Data.SQLite.SQLiteConnection p_Connection = new System.Data.SQLite.SQLiteConnection();
		    this.OpenConnection(strConn, ref p_Connection);
            if (this.m_intError == 0)
            {
                using (SQLiteCommand command = new SQLiteCommand(p_Connection))
                {
                    
                    command.CommandText = strSQL;
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (System.Threading.ThreadInterruptedException err)
                    {
                        m_strError = err.Message;
                    }
                    catch (System.Threading.ThreadAbortException err)
                    {
                        m_strError = err.Message;
                    }
                    catch (Exception caught)
                    {
                        this.m_strError = caught.Message + " The SQL command " + strSQL + " Failed"; ;
                        this.m_intError = -1;
                        if (_bDisplayErrors)
                            MessageBox.Show("!!Error!! \n" +
                                "Module - SQLite:SqlNonQuery  \n" +
                                "Err Msg - " + this.m_strError,
                                "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Exclamation);
                    }
                    
                }
                CloseAndDisposeConnection(p_Connection,false);
            }
            p_Connection = null;
		}
		public void SqlNonQuery(System.Data.SQLite.SQLiteConnection p_Connection, string strSQL)
		{
            this.m_intError = 0;
            this.m_strError = "";
            using (SQLiteCommand command = new SQLiteCommand(p_Connection))
            {
                command.CommandText = strSQL;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (System.Threading.ThreadInterruptedException err)
                {
                    m_strError = err.Message;
                }
                catch (System.Threading.ThreadAbortException err)
                {
                    m_strError = err.Message;
                }
                catch (Exception caught)
                {
                    this.m_strError = caught.Message + " The SQL command " + strSQL + " Failed"; ;
                    this.m_intError = -1;
                    if (_bDisplayErrors)
                        MessageBox.Show("!!Error!! \n" +
                            "Module - SQLite:SqlNonQuery  \n" +
                            "Err Msg - " + this.m_strError,
                            "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Exclamation);
                }

            }

			
		}

		public void SqlQueryReader(string strConn,string strSql)
		{
			this.m_intError=0;
			this.m_strError="";
            this.OpenConnection(strConn);
			if (this.m_intError==0)
			{
			    this.m_Command = this.m_Connection.CreateCommand();
				this.m_Command.CommandText = strSql;
                try
                {
                    this.m_DataReader = this.m_Command.ExecuteReader();
                }
                catch (System.Threading.ThreadInterruptedException err)
                {
                    m_strError = err.Message;
                }
                catch (System.Threading.ThreadAbortException err)
                {
                    m_strError = err.Message;
                }
                catch (Exception caught)
                {
                    this.m_intError = -1;
                    this.m_strError = caught.Message + " The Query Command " + this.m_Command.CommandText.ToString() + " Failed";
                    if (_bDisplayErrors)
                        MessageBox.Show("!!Error!! \n" +
                            "Module - SQLite:SqlQueryReader  \n" +
                            "Err Msg - " + this.m_strError,
                            "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Exclamation);
                    this.m_DataReader = null;
                    this.m_Command = null;
                    this.m_Connection.Close();
                    this.m_Connection = null;
                    return;
                }
                
			}
		}
		public void SqlQueryReader(System.Data.SQLite.SQLiteConnection p_Connection,string strSql)
		{
			this.m_intError=0;
			this.m_strError="";
			
				this.m_Command = p_Connection.CreateCommand();
				this.m_Command.CommandText = strSql;
				try
				{
					this.m_DataReader = this.m_Command.ExecuteReader();
				}
				catch (System.Threading.ThreadInterruptedException err)
				{
                    m_strError = err.Message;
				}
				catch (System.Threading.ThreadAbortException err)
				{
                    m_strError = err.Message;
				}
				catch (Exception caught)
				{
					this.m_intError = -1;
					this.m_strError = caught.Message + " The Query Command " + this.m_Command.CommandText.ToString() + " Failed";
					if (_bDisplayErrors)
					MessageBox.Show("!!Error!! \n" + 
						"Module - SQLite:SqlQueryReader  \n" + 
						"Err Msg - " + this.m_strError,
                        "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
						System.Windows.Forms.MessageBoxIcon.Exclamation);
					this.m_DataReader = null;
					this.m_Command = null;
					return;
				}
			    
			
		}
		public void SqlQueryReader(System.Data.SQLite.SQLiteConnection p_Connection,System.Data.SQLite.SQLiteTransaction p_trans,string strSql)
		{
			this.m_intError=0;
			this.m_strError="";
			
			this.m_Command = p_Connection.CreateCommand();
			this.m_Command.CommandText = strSql;
            this.m_Command.Transaction = p_trans;
			try
			{
				this.m_DataReader = this.m_Command.ExecuteReader();
			}
			catch (System.Threading.ThreadInterruptedException err)
			{
                m_strError = err.Message;
			}
			catch (System.Threading.ThreadAbortException err)
			{
                m_strError = err.Message;
			}
			catch (Exception caught)
			{
				this.m_intError = -1;
				this.m_strError = caught.Message + " The Query Command " + this.m_Command.CommandText.ToString() + " Failed";
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:SqlQueryReader  \n" + 
					"Err Msg - " + this.m_strError,
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				this.m_DataReader = null;
				this.m_Command = null;
				return;
			}
		}
		public bool FieldExist(System.Data.SQLite.SQLiteConnection p_oConn, string p_strSql, string p_strField)
		{
			string strDelimiter=",";
			string strList = getFieldNames(p_oConn,p_strSql);
			string[] strArray = strList.Split(strDelimiter.ToCharArray());
			for (int x=0;x<=strArray.Length-1;x++)
			{
				if (strArray[x] != null && strArray[x].Trim().Length > 0)
				{
					if (strArray[x].Trim().ToUpper()==p_strField.Trim().ToUpper()) return true;
				}
			}
			return false;

		}
		public bool FieldExist(string p_strConn, string p_strSql, string p_strField)
		{
			string strDelimiter=",";
			this.OpenConnection(p_strConn);
			if (this.m_intError==0)
			{
				string strList = getFieldNames(this.m_Connection,p_strSql);
				string[] strArray = strList.Split(strDelimiter.ToCharArray());
				for (int x=0;x<=strArray.Length-1;x++)
				{
					if (strArray[x] != null && strArray[x].Trim().Length > 0)
					{
						if (strArray[x].Trim().ToUpper()==p_strField.Trim().ToUpper())
						{
							this.m_Connection.Close();
							return true;
						}
					}
				}
				this.m_Connection.Close();

			}
			return false;

		}

		public string getFieldNames(System.Data.SQLite.SQLiteConnection p_oConn,string p_strSql)
		{
			this.m_intError=0;
			System.Data.DataTable oTableSchema = this.getTableSchema(p_oConn,p_strSql);
			if (this.m_intError !=0) return "";
			string strFields="";
			
			for (int x=0; x<=oTableSchema.Rows.Count-1;x++)
			{
				strFields = strFields + oTableSchema.Rows[x]["columnname"].ToString().Trim() + ",";
			}
			if (strFields.Trim().Length > 0) strFields=strFields.Substring(0,strFields.Trim().Length -1);

			return strFields;
			
		}
		public string getFieldNames(string p_strConn,string p_strSql)
		{
			string strFields="";
			this.m_intError=0;
			this.OpenConnection(p_strConn);
			if (this.m_intError==0)
			{
				System.Data.DataTable oTableSchema = this.getTableSchema(this.m_Connection,p_strSql);
				if (this.m_intError !=0) return "";
				
			
				for (int x=0; x<=oTableSchema.Rows.Count-1;x++)
				{
					strFields = strFields + oTableSchema.Rows[x]["columnname"].ToString().Trim() + ",";
				}
				if (strFields.Trim().Length > 0) strFields=strFields.Substring(0,strFields.Trim().Length -1);

				
			}
			return strFields;
			
		}

		public System.Data.DataTable getTableSchema(System.Data.SQLite.SQLiteConnection p_Connection,string strSql)
		{
            System.Data.DataTable p_dt;
            this.m_intError = 0;
            this.m_strError = "";

            this.m_Command = p_Connection.CreateCommand();
            this.m_Command.CommandText = strSql;
            try
            {
                this.m_DataReader = this.m_Command.ExecuteReader();
                p_dt = this.m_DataReader.GetSchemaTable();
            }
            catch (Exception caught)
            {
                this.m_intError = -1;
                this.m_strError = caught.Message + " The Query Command " + this.m_Command.CommandText.ToString() + " Failed";
                if (_bDisplayErrors)
                    MessageBox.Show("!!Error!! \n" +
                        "Module - SQLite:getTableSchema  \n" +
                        "Err Msg - " + this.m_strError,
                        "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Exclamation);
                this.m_DataReader = null;
                this.m_Command = null;
                return null;
            }
            this.m_DataReader.Close();
            return p_dt;		
		}
		
        public System.Data.DataTable getTableSchema(System.Data.SQLite.SQLiteConnection p_Connection,
			                                        System.Data.SQLite.SQLiteTransaction p_trans,
			                                        string strSql)
		{
			System.Data.DataTable p_dt;
			this.m_intError=0;
			this.m_strError="";
			
			this.m_Command = p_Connection.CreateCommand();
			this.m_Command.CommandText = strSql;
			this.m_Command.Transaction = p_trans;
			try
			{
				this.m_DataReader = this.m_Command.ExecuteReader();
				p_dt = this.m_DataReader.GetSchemaTable();
			}
			catch (Exception caught)
			{
				this.m_intError = -1;
				this.m_strError = caught.Message + " The Query Command " + this.m_Command.CommandText.ToString() + " Failed";
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:getTableSchema  \n" + 
					"Err Msg - " + this.m_strError,
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				this.m_DataReader = null;
				this.m_Command = null;
				return null;
			}
			this.m_DataReader.Close();
			return p_dt;
			
		}

		public System.Data.DataTable getTableSchema(string strConn,string strSql)
		{
			System.Data.DataTable p_dt;
			this.m_intError=0;
			this.m_strError="";
			
			this.OpenConnection(strConn);
			if (this.m_intError==0)
			{
				this.m_Command = this.m_Connection.CreateCommand();
				this.m_Command.CommandText = strSql;
				try
				{
					this.m_DataReader = this.m_Command.ExecuteReader(CommandBehavior.KeyInfo);
					p_dt = this.m_DataReader.GetSchemaTable();
				}
				catch (Exception caught)
				{
					this.m_intError = -1;
					this.m_strError = caught.Message + " The Query Command " + this.m_Command.CommandText.ToString() + " Failed";
					if (_bDisplayErrors)
					MessageBox.Show("!!Error!! \n" + 
						"Module - SQLite:getTableSchema  \n" + 
						"Err Msg - " + this.m_strError,
                        "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
						System.Windows.Forms.MessageBoxIcon.Exclamation);
					this.m_DataReader = null;
					this.m_Command = null;
					return null;
				}
				this.m_DataReader.Close();
				return p_dt;
			}
			return null;
			
			
		}
		public void getTableSchema2(string strConn,string strSql)
		{
			this.m_intError=0;
			this.m_strError="";
			if (this.m_DataTable != null) this.m_DataTable.Clear();
			
			this.OpenConnection(strConn);
			if (this.m_intError==0)
			{
				this.m_Command = this.m_Connection.CreateCommand();
				this.m_Command.CommandText = strSql;
				try
				{
					this.m_DataReader = this.m_Command.ExecuteReader(CommandBehavior.KeyInfo);
					this.m_DataTable = this.m_DataReader.GetSchemaTable();
				}
				catch (Exception caught)
				{
					this.m_intError = -1;
					this.m_strError = caught.Message + " The Query Command " + this.m_Command.CommandText.ToString() + " Failed";
					if (_bDisplayErrors)
						MessageBox.Show("!!Error!! \n" + 
							"Module - SQLite:getTableSchema  \n" + 
							"Err Msg - " + this.m_strError,
                            "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
							System.Windows.Forms.MessageBoxIcon.Exclamation);
					this.m_DataReader = null;
					this.m_Command = null;
				}
				this.m_DataReader.Close();
			}
		}
		public void getTableSchema2(System.Data.SQLite.SQLiteConnection p_Connection,string strSql)
		{
			this.m_intError=0;
			this.m_strError="";
			if (this.m_DataTable != null) this.m_DataTable.Clear();
			
			if (this.m_intError==0)
			{
				this.m_Command = p_Connection.CreateCommand();
				this.m_Command.CommandText = strSql;
				try
				{
					this.m_DataReader = this.m_Command.ExecuteReader(CommandBehavior.KeyInfo);
					this.m_DataTable = this.m_DataReader.GetSchemaTable();
				}
				catch (Exception caught)
				{
					this.m_intError = -1;
					this.m_strError = caught.Message + " The Query Command " + this.m_Command.CommandText.ToString() + " Failed";
					if (_bDisplayErrors)
						MessageBox.Show("!!Error!! \n" + 
							"Module - SQLite:getTableSchema  \n" + 
							"Err Msg - " + this.m_strError,
                            "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
							System.Windows.Forms.MessageBoxIcon.Exclamation);
					this.m_DataReader = null;
					this.m_Command = null;
				}
				this.m_DataReader.Close();
			}
		}
        public List<string> getStringList(System.Data.SQLite.SQLiteConnection p_Connection, string p_strSQL)
        {
            List<string> strList = new List<string>();
            SqlQueryReader(p_Connection, p_strSQL);
            if (m_intError == 0 && m_DataReader.HasRows)
            {
                while (m_DataReader.Read())
                {
                    if (m_DataReader != null && m_DataReader[0] != DBNull.Value)
                    {
                        strList.Add(m_DataReader[0].ToString());
                    }
                }
                m_DataReader.Close();
            }
            return strList;
        }
        public void getStringList(System.Data.SQLite.SQLiteConnection p_Connection, string p_strSQL, ref List<string> p_strList)
        {
            SqlQueryReader(p_Connection, p_strSQL);
            if (m_intError == 0 && m_DataReader.HasRows)
            {
                while (m_DataReader.Read())
                {
                    if (m_DataReader != null && m_DataReader[0] != DBNull.Value)
                    {
                        p_strList.Add(m_DataReader[0].ToString());
                    }
                }
                m_DataReader.Close();
            }
        }
       
		/****
		 **** format strings to be used in an sql statement
		 ****/
		public string FixString(string SourceString , string StringToReplace, string StringReplacement)
		{
			SourceString = SourceString.Replace(StringToReplace, StringReplacement);
			return(SourceString);
		}
		//returns Y or N for whether the field is a string or not
		public string getIsTheFieldAStringDataType(string strFieldType)
		{
			switch (strFieldType.Trim())
			{
				case "System.Single":
					return "N";
				case "System.Double":
					return "N";
				case "System.Decimal":
					return "N";
				case "System.String":
					return "Y";
				case "System.Int16":
					return "N";
				case "System.Char":
					return "Y";
				case "System.Int32":
					return "N";
				case "System.DateTime":
					return "Y";
				case "System.DayOfWeek":
					return "Y";
				case "System.Int64":
					return "N";
				case "System.Byte":
					return "N";
				case "System.Boolean":
					return "N";
				default:
					//return "N";
					MessageBox.Show(strFieldType + " is undefined");
					return "N";
			}


		}
        public string FormatCreateTableSqlFieldItemForMSAccess(System.Data.DataRow p_oRow)
        {
            string strLine;
            string strColumn = p_oRow["ColumnName"].ToString().Trim();
            string strDataType = p_oRow["DataType"].ToString().Trim();
            string strPrecision = "";
            string strScale = "";
            string strSize = "";

            if (p_oRow["ColumnSize"] != null)
               strSize = Convert.ToString(p_oRow["ColumnSize"]);

            if (p_oRow["NumericPrecision"] != null)
                strPrecision = Convert.ToString(p_oRow["NumericPrecision"]);

            if (p_oRow["NumericScale"] != null)
                strScale = Convert.ToString(p_oRow["NumericScale"]);

            if (strColumn.Trim().ToUpper() == "VALUE" ||
                strColumn.Trim().ToUpper() == "USE" ||
                strColumn.Trim().ToUpper() == "YEAR")
                strColumn = "`" + strColumn.Trim() + "`";

            
            switch (strDataType)
            {

                case "System.Single":
                    strDataType = "single";
                    break;
                case "System.Double":
                    strDataType = "double";
                    break;
                case "System.Decimal":
                    strDataType = "decimal";
                    break;
                case "System.String":
                    strDataType = "text";
                    break;
                case "System.Int16":
                    strDataType = "short";
                    break;
                case "System.Char":
                    strDataType = "text";
                    break;
                case "System.Int32":
                    strDataType = "integer";
                    break;
                case "System.DateTime":
                    strDataType = "datetime";
                    break;
                case "System.DayOfWeek":
                    break;
                case "System.Int64":
                    break;
                case "System.Byte":
                    strDataType="byte";
                    break;
                case "System.Boolean":
                    break;



            }

            strLine = strColumn + " " + strDataType;

            if (strSize.Trim().Length > 0 && strDataType == "text")
                if (Convert.ToInt32(strSize) < 256)
                    strLine = strLine + " (" + strSize + ")";
                else
                {
                    strLine = strColumn + " memo";
                }
            else
            {
                if (strDataType == "decimal")
                {
                    if (strPrecision.Trim() == "0")
                        strLine = strColumn + " double";
                    else 
                        strLine = strLine + " (" + strPrecision + "," + strScale + ")";
                }
                
                    
            }
            return strLine;

        }

        public string FormatSelectSqlFieldItemForMSAccess(System.Data.DataRow p_oRow)
        {
            string strLine="";
            string strColumn = p_oRow["ColumnName"].ToString().Trim();
            string strDataType = p_oRow["DataType"].ToString().Trim();
            string strPrecision = "";
            string strScale = "";
            string strSize = "";

            if (p_oRow["ColumnSize"] != null)
                strSize = Convert.ToString(p_oRow["ColumnSize"]);

            if (p_oRow["NumericPrecision"] != null)
                strPrecision = Convert.ToString(p_oRow["NumericPrecision"]);

            if (p_oRow["NumericScale"] != null)
                strScale = Convert.ToString(p_oRow["NumericScale"]);

            //if (strColumn.Trim().ToUpper() == "VALUE" ||
            //    strColumn.Trim().ToUpper() == "USE")
            //    strColumn = "`" + strColumn.Trim() + "`";




            switch (strDataType)
            {

                case "System.Single":
                    strDataType = "single";
                    break;
                case "System.Double":
                    strDataType = "double";
                    break;
                case "System.Decimal":
                    strDataType = "decimal";
                    break;
                case "System.String":
                    strDataType = "text";
                    break;
                case "System.Int16":
                    strDataType = "short";
                    break;
                case "System.Char":
                    strDataType = "text";
                    break;
                case "System.Int32":
                    strDataType = "integer";
                    break;
                case "System.DateTime":
                    strDataType = "datetime";
                    break;
                case "System.DayOfWeek":
                    break;
                case "System.Int64":
                    break;
                case "System.Byte":
                    strDataType="byte";
                    break;
                case "System.Boolean":
                    break;



            }

            strLine = strColumn;

            if (strDataType == "decimal")
            {
                if (strPrecision.Trim() == "0")
                    strLine = "ROUND(" + strColumn + ",14) AS " + strColumn;
            }
            else if (strDataType == "double")
            {
                strLine = "ROUND(" + strColumn + ",14) AS " + strColumn;
            }
          

            return strLine;

        }

            
		public void CreateDataSet(string strConn,
			string strSQL,string strTableName)
		{
			this.m_intError=0;
			this.m_strError="";
			try
			{
				this.OpenConnection(strConn);
				if (this.m_intError == 0)
				{
					this.m_DataAdapter = new System.Data.SQLite.SQLiteDataAdapter(strSQL, this.m_Connection);
					this.m_DataSet = new DataSet();
					this.m_DataAdapter.Fill(this.m_DataSet,strTableName);
					this.m_Connection.Close();
				}

			}
			catch (Exception caught)
			{
				this.m_intError = -1;
				this.m_strError = caught.Message + " : SQL query command " + strSQL + " failed" ;
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:CreateDataSet  \n" + 
					"Err Msg - " + this.m_strError,
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				this.m_Connection.Close();
				this.m_DataAdapter = null;
				this.m_DataSet  = null;
				return;
			}
			
		}

		public void CreateDataSet(System.Data.SQLite.SQLiteConnection p_conn,
			string strSQL,string strTableName)
		{
			this.m_intError=0;
			this.m_strError="";
			try
			{
					this.m_DataAdapter = new System.Data.SQLite.SQLiteDataAdapter(strSQL, p_conn);
					this.m_DataSet = new DataSet();
					this.m_DataAdapter.Fill(this.m_DataSet,strTableName);
					//this.m_Connection.Close();
			}
			catch (Exception caught)
			{
				this.m_intError = -1;
				this.m_strError = caught.Message + " : SQL query command " + strSQL + " failed" ;
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:CreateDataSet  \n" + 
					"Err Msg - " + this.m_strError,
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				//this.m_Connection.Close();
				this.m_DataAdapter = null;
				this.m_DataSet  = null;
				return;
			}
			
		}

		public void AddSQLQueryToDataSet(System.Data.SQLite.SQLiteConnection p_conn,
			ref System.Data.SQLite.SQLiteDataAdapter p_da,
			ref System.Data.DataSet p_ds,
			string strSQL, 
			string strTableName)
		{
			this.m_intError=0;
			this.m_strError="";
			System.Data.SQLite.SQLiteCommand p_Command;
			try
			{
				p_Command = p_conn.CreateCommand();
				p_Command.CommandText = strSQL;
				p_da.SelectCommand = p_Command;
				p_da.Fill(p_ds,strTableName);
			}
			catch (Exception caught)
			{
				this.m_intError = -1;
				this.m_strError = caught.Message + " : SQL query command " + strSQL + " failed" ;
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:AddSQLQueryToDataSet  \n" + 
					"Err Msg - " + this.m_strError,
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				if (_bDisplayErrors)
				MessageBox.Show(this.m_strError);
			}

		}
		public void AddSQLQueryToDataSet(System.Data.SQLite.SQLiteConnection p_conn,
			ref System.Data.SQLite.SQLiteDataAdapter p_da,
			ref System.Data.DataSet p_ds,
			ref System.Data.SQLite.SQLiteTransaction p_trans,
			string strSQL, 
			string strTableName)
		{
			this.m_intError=0;
			this.m_strError="";
			System.Data.SQLite.SQLiteCommand p_Command;
			try
			{
				p_Command = p_conn.CreateCommand();
				p_Command.CommandText = strSQL;
				p_Command.Transaction = p_trans;
				p_da.SelectCommand = p_Command;
				p_da.Fill(p_ds,strTableName);
			}
			catch (Exception caught)
			{
				this.m_intError = -1;
				this.m_strError = caught.Message + " : SQL query command " + strSQL + " failed" ;
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:AddSQLQueryToDataSet  \n" + 
					"Err Msg - " + this.m_strError,
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				if (_bDisplayErrors)
				MessageBox.Show(this.m_strError);
			}

		}

		public void DatasetSQLInsertCommand(System.Data.SQLite.SQLiteConnection p_conn,
			ref System.Data.SQLite.SQLiteDataAdapter p_da,
			ref System.Data.DataSet p_ds,
			string strSQL, 
			string strTableName)
		{

		}

		public long getRecordCount(string strConn,
			string strSQL,string strTableName)
		{
			System.Data.SQLite.SQLiteConnection p_Conn;
			System.Data.SQLite.SQLiteCommand p_Command;
			long intRecTtl=0;
			this.m_intError=0;
			this.m_strError="";
			p_Conn = new System.Data.SQLite.SQLiteConnection();
			try
			{
				this.OpenConnection(strConn, ref p_Conn);
				if (this.m_intError == 0)
				{
					p_Command = p_Conn.CreateCommand();
					p_Command.CommandText = strSQL;
					intRecTtl = Convert.ToInt32(p_Command.ExecuteScalar());
				}

			}
			catch (Exception caught)
			{
				this.m_intError = -1;
				this.m_strError = caught.Message + "  SQL query command: " + strSQL + " failed" ;
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:getRecordCount  \n" + 
					"Err Msg - " + this.m_strError,
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				p_Conn.Close();
				if (_bDisplayErrors) MessageBox.Show(this.m_strError);
			}
			try
			{
				p_Conn.Close();
			}
			catch 
			{
			}
			p_Conn = null;
			p_Command = null;
			return intRecTtl;
			
		}

		public long getRecordCount(System.Data.SQLite.SQLiteConnection p_conn,
			string strSQL,string strTableName)
		{
			System.Data.SQLite.SQLiteCommand p_Command;
			long intRecTtl=0;
			this.m_intError=0;
			this.m_strError="";
			try
			{
				
					p_Command = p_conn.CreateCommand();
					p_Command.CommandText = strSQL;
					intRecTtl = Convert.ToInt32(p_Command.ExecuteScalar());
				

			}
			catch (Exception caught)
			{
				this.m_intError = -1;
				this.m_strError = caught.Message + "  SQL query command: " + strSQL + " failed" ;
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:getRecordCount  \n" + 
					"Err Msg - " + this.m_strError,
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
			}
			p_Command = null;
			return intRecTtl;
			
		}
		public long getRecordCount(System.Data.SQLite.SQLiteConnection p_conn, 
			System.Data.SQLite.SQLiteTransaction p_trans,
			string strSQL,string strTableName)
		{
			System.Data.SQLite.SQLiteCommand p_Command;
			long intRecTtl=0;
			this.m_intError=0;
			this.m_strError="";
			try
			{
				
				p_Command = p_conn.CreateCommand();
				p_Command.CommandText = strSQL;
				p_Command.Transaction = p_trans;
				intRecTtl = Convert.ToInt32(p_Command.ExecuteScalar());
				

			}
			catch (Exception caught)
			{
				this.m_intError = -1;
				this.m_strError = caught.Message + "  SQL query command: " + strSQL + " failed" ;
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:getRecordCount  \n" + 
					"Err Msg - " + this.m_strError,
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				
			}
			p_Command = null;
			return intRecTtl;
			
		}
		public string getSQLiteConnString(string strDataSourceDbFile)
		{
			return "Data Source=" + strDataSourceDbFile.Trim();
			
		}
	
		public string CreateSQLNOTINString(System.Data.SQLite.SQLiteConnection p_conn,string strSQL)
		{
			string str = "";
			this.SqlQueryReader(p_conn, strSQL);
			if (this.m_intError == 0)
			{
				if (this.m_DataReader.HasRows)
				{
					while (this.m_DataReader.Read())
					{
						if (str.Trim().Length == 0)
						{
							str = this.m_DataReader[0].ToString().Trim();
						}
						else
						{
						    str += "," + this.m_DataReader[0].ToString().Trim();
						}
					}
				}
				this.m_DataReader.Close();
			}
			return str;
			
			
		}
		public string getSingleStringValueFromSQLQuery(System.Data.SQLite.SQLiteConnection p_conn,
			string strSQL,string strTableName)
		{
			System.Data.SQLite.SQLiteCommand p_Command;
			string strValue="";
			this.m_intError=0;
			this.m_strError="";
			try
			{
				
				p_Command = p_conn.CreateCommand();
				p_Command.CommandText = strSQL;
				strValue = Convert.ToString(p_Command.ExecuteScalar());
				

			}
			catch (Exception caught)
			{
				this.m_intError = -1;
				this.m_strError = caught.Message + "  SQL query command: " + strSQL + " failed" ;
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:getSingleStringValueFromSQLQuery  \n" + 
					"Err Msg - " + this.m_strError,
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				
			}
			p_Command = null;
			return strValue;

		}
		public string getSingleStringValueFromSQLQuery(System.Data.SQLite.SQLiteConnection p_conn,
			System.Data.SQLite.SQLiteTransaction p_trans, string strSQL,string strTableName)
		{
			System.Data.SQLite.SQLiteCommand p_Command;
			string strValue="";
			this.m_intError=0;
			this.m_strError="";
			try
			{
				
				p_Command = p_conn.CreateCommand();
				p_Command.CommandText = strSQL;
				p_Command.Transaction = p_trans;
				strValue = Convert.ToString(p_Command.ExecuteScalar());
				

			}
			catch (Exception caught)
			{
				this.m_intError = -1;
				this.m_strError = caught.Message + "  SQL query command: " + strSQL + " failed" ;
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:getSingleStringValueFromSQLQuery  \n" + 
					"Err Msg - " + this.m_strError,
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				
				
				
			}
			p_Command = null;
			return strValue;

		}
		/// <summary>
		/// Execute a query that returns a single string value
		/// </summary>
		/// <param name="strConn">Access connection string</param>
		/// <param name="strSQL">SQL that returns a single string value</param>
		/// <param name="strTableName">table name</param>
		/// <returns></returns>
		public string getSingleStringValueFromSQLQuery(string strConn,
			string strSQL,string strTableName)
		{
			System.Data.SQLite.SQLiteConnection oOleDbConn;
			System.Data.SQLite.SQLiteCommand oOleDbCommand;
			string strValue="";
			this.m_intError=0;
			this.m_strError="";
			try
			{
				oOleDbConn = new System.Data.SQLite.SQLiteConnection();
				this.OpenConnection(strConn, ref oOleDbConn);
				if (m_intError==0)
				{
					oOleDbCommand = oOleDbConn.CreateCommand();
					oOleDbCommand.CommandText = strSQL;
					strValue = Convert.ToString(oOleDbCommand.ExecuteScalar());
					oOleDbConn.Close();
				}
				

			}
			catch (Exception caught)
			{
				this.m_intError = -1;
				this.m_strError = caught.Message + "  SQL query command: " + strSQL + " failed" ;
				if (_bDisplayErrors)
					MessageBox.Show("!!Error!! \n" + 
						"Module - SQLite:getSingleStringValueFromSQLQuery  \n" + 
						"Err Msg - " + this.m_strError,
                        "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
						System.Windows.Forms.MessageBoxIcon.Exclamation);
				
			}
			oOleDbCommand = null;
			oOleDbConn=null;
			return strValue;

		}
        /// <summary>
        /// return a single numeric double value resulting from SQL command 
        /// </summary>
        /// <param name="p_conn"></param>
        /// <param name="strSQL"></param>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        public double getSingleDoubleValueFromSQLQuery(System.Data.SQLite.SQLiteConnection p_conn, string strSQL, string strTableName)
        {
            double dblValue = -1;
            this.m_intError = 0;
            this.m_strError = "";
            using (var p_Command = new SQLiteCommand(strSQL, p_conn))
            {
                try
                {
                    dblValue = Convert.ToDouble(p_Command.ExecuteScalar());
                }
                catch (Exception caught)
                {
                    this.m_intError = -1;
                    this.m_strError = caught.Message + "  SQL query command: " + strSQL + " failed";
                    if (_bDisplayErrors)
                        MessageBox.Show("!!Error!! \n" +
                                        "Module - ado_data_access:getSingleStringValueFromSQLQuery  \n" +
                                        "Err Msg - " + this.m_strError,
                            "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Exclamation);
                }
            }
            return dblValue;

        }

		public System.Data.DataTable ConvertDataViewToDataTable(
			System.Data.DataView p_dv)
		{
			int x=0;
			System.Data.DataTable p_dtNew;
			//copy exact structure from the view to the new table
			p_dtNew = p_dv.Table.Clone();
			int idx = 0;
			//create an array containing all the column names in the new data table
			string[] strColNames = new string[p_dtNew.Columns.Count];
			for (x=0;x<=p_dtNew.Columns.Count-1;x++)
			{
				strColNames[idx++] = p_dtNew.Columns[x].ColumnName;
			}
			//append each row in the dataview to the new table
			System.Collections.IEnumerator viewEnumerator = p_dv.GetEnumerator();
			
			while (viewEnumerator.MoveNext())
			{
				DataRowView drv = (DataRowView)viewEnumerator.Current;
				DataRow dr = p_dtNew.NewRow();
				try
				{
					foreach (string strName in strColNames)
					{
						//value in data table row and column equal to value in 
						//dataview row and column value
						dr[strName] = drv[strName];
						
					}
				}
				catch (System.Threading.ThreadInterruptedException err)
				{
                    m_strError = err.Message;
				}
				catch (System.Threading.ThreadAbortException err)
				{
                    m_strError = err.Message;
				}
				catch (Exception ex)
				{
				if (_bDisplayErrors)	
					MessageBox.Show("!!Error!! \n" + 
						"Module - SQLite:ConvertDataViewToDataTable  \n" + 
						"Err Msg - " + ex.Message,
						"FIA Biosum",System.Windows.Forms.MessageBoxButtons.OK,
						System.Windows.Forms.MessageBoxIcon.Exclamation);
				
				}
				//append the new row to the data table
				p_dtNew.Rows.Add(dr);
			}
			return p_dtNew;
		}
		/// <summary>
		/// Converts a given delimited file into a dataset. 
		/// Assumes that the first line    
		/// of the text file contains the column names.
		/// </summary>
		/// <param name="File">The name of the file to open</param>    
		/// <param name="TableName">The name of the 
		/// Table to be made within the DataSet returned</param>
		/// <param name="delimiter">The string to delimit by</param>
		/// <returns></returns>  
		public void ConvertDelimitedTextToDataTable(System.Data.DataSet p_ds, 
			                                            string p_strFile, 
                                             			string p_strTableName, string p_strDelimiter)
		{   
            this.m_intError=0;
			try
			{
				//Open the file in a stream reader.
				StreamReader s = new StreamReader(p_strFile);
        
				//Split the first line into the columns       
				string[] columns = s.ReadLine().Split(p_strDelimiter.ToCharArray());
  
				//Add the new DataTable to the RecordSet
				p_ds.Tables.Add(p_strTableName);
    
				//Cycle the colums, adding those that don't exist yet 
				//and sequencing the one that do.
				foreach(string col in columns)
				{
					bool added = false;
					string next = "";
					int i = 0;
					while(!added)        
					{
						//Build the column name and remove any unwanted characters.
						string columnname = col + next;
						columnname = columnname.Replace("#","");
						columnname = columnname.Replace("'","");
						columnname = columnname.Replace("&","");
						columnname = columnname.Replace("\"","");
        
						//See if the column already exists
						if(!p_ds.Tables[p_strTableName].Columns.Contains(columnname))
						{
							//if it doesn't then we add it here and mark it as added
							p_ds.Tables[p_strTableName].Columns.Add(columnname);
							added = true;
						}
						else
						{
							//if it did exist then we increment the sequencer and try again.
							i++;  
							next = "_" + i.ToString();
						}         
					}
				}
    
				//Read the rest of the data in the file.        
				string AllData = s.ReadToEnd();
    
				//Split off each row at the Carriage Return/Line Feed
				//Default line ending in most <A class=iAs style="FONT-WEIGHT: normal; FONT-SIZE: 100%; PADDING-BOTTOM: 1px; COLOR: darkgreen; BORDER-BOTTOM: darkgreen 0.07em solid; BACKGROUND-COLOR: transparent; TEXT-DECORATION: underline" href="#" target=_blank itxtdid="2592535">windows</A> exports.  
				//You may have to edit this to match your particular file.
				//This will work for Excel, Access, etc. default exports.
				string[] rows = AllData.Split("\r\n".ToCharArray());
 
				//Now add each row to the DataSet        
				foreach(string r in rows)
				{
					//Split the row at the delimiter.
					string[] items = r.Split(p_strDelimiter.ToCharArray());
      
					//Add the item
					p_ds.Tables[p_strTableName].Rows.Add(items);  
				}
			}
			catch (Exception caught)
			{

				this.m_intError=-1;
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:ConvertDelimitedTextToDataTable  \n" + 
					"Err Msg - " + caught.Message.ToString().Trim(),
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				this.m_intError=-1;
			}
    
			//Return the imported data.        

		}
		/// <summary>
		/// Create an oledb data adapter insert command. The select sql statement
		/// is used to get the data types of the fields used in the insert.
		/// </summary>
		/// <param name="p_conn">the oledb database connection</param>
		/// <param name="p_da">the data adapter</param>
		/// <param name="p_trans">oledb transaction object</param>
		/// <param name="p_strSQL">select sql statement containing fields in the insert command</param>
		/// <param name="p_strTable">table name that records are inserted</param>
		public void ConfigureDataAdapterInsertCommand(System.Data.SQLite.SQLiteConnection p_conn, 
			                                          System.Data.SQLite.SQLiteDataAdapter p_da,
													   System.Data.SQLite.SQLiteTransaction p_trans,
			                                           string p_strSQL,string p_strTable)
		{

			this.m_intError=0;
			System.Data.DataTable p_dtTableSchema = this.getTableSchema(p_conn,p_trans, p_strSQL);
			if (this.m_intError !=0) return;
			string strFields = "";
			string strValues = "";
			int x;
			try
			{
				//Build the plot insert sql
				for (x=0; x<=p_dtTableSchema.Rows.Count-1;x++)
				{
					if (strFields.Trim().Length == 0)
					{
						strFields = "(";
					}
					else
					{	
						strFields = strFields + "," ;
					}
					strFields = strFields + p_dtTableSchema.Rows[x]["columnname"].ToString().Trim();
					if (strValues.Trim().Length == 0)
					{
						strValues = "(";
					}
					else
					{	
						strValues = strValues + ",";
					}
					strValues = strValues + "?";

				}
				strFields = strFields + ")";
				strValues = strValues + ");";
				//create an insert command 
				p_da.InsertCommand = p_conn.CreateCommand();
				//bind the transaction object to the insert command
				p_da.InsertCommand.Transaction = p_trans;
				p_da.InsertCommand.CommandText = 
					"INSERT INTO " + p_strTable + " "  + strFields + " VALUES " + strValues;
				//define field datatypes for the data adapter
				for (x=0; x<=p_dtTableSchema.Rows.Count-1;x++)
				{
					strFields=p_dtTableSchema.Rows[x]["columnname"].ToString().Trim();
					switch (p_dtTableSchema.Rows[x]["datatype"].ToString().Trim())
					{
						case "System.String" :
							p_da.InsertCommand.Parameters.Add
								(strFields,
								System.Data.DbType.String,
								0,
								strFields);
							break;
						case "System.Double":
							p_da.InsertCommand.Parameters.Add
								(strFields, 
								System.Data.DbType.Double,
								0,
								strFields);
							break;
						//case "System.Boolean":
						//	p_da.InsertCommand.Parameters.Add
						//		(strFields, 
						//		System.Data.DbType.Boolean,
						//		0,
						//		strFields);
						//	break;
						case "System.DateTime":
							p_da.InsertCommand.Parameters.Add
								(strFields, 
								System.Data.DbType.DateTime,
								0,
								strFields);
							break;
						case "System.Decimal":
							p_da.InsertCommand.Parameters.Add
								(strFields, 
								System.Data.DbType.Double,
								0,
								strFields);
							break;
						case "System.Int16":
							p_da.InsertCommand.Parameters.Add
								(strFields, 
								System.Data.DbType.Int16,
								0,
								strFields);
							break;
						case "System.Int32":
							p_da.InsertCommand.Parameters.Add
								(strFields, 
								System.Data.DbType.Int32,
								0,
								strFields);
							break;
						case "System.Int64":
							p_da.InsertCommand.Parameters.Add
								(strFields, 
								System.Data.DbType.Int64,
								0,
								strFields);
							break;
						case "System.SByte":
							p_da.InsertCommand.Parameters.Add
								(strFields, 
								System.Data.DbType.SByte,
								0,
								strFields);
							break;
						case "System.Byte":
							p_da.InsertCommand.Parameters.Add
								(strFields, 
								System.Data.DbType.Byte,
								0,
								strFields);
							break;
						case "System.Single":
							p_da.InsertCommand.Parameters.Add
								(strFields, 
								System.Data.DbType.Single,
								0,
								strFields);
							break;
						default:
							MessageBox.Show("Could Not Set Data Adapter Parameter For DataType " + p_dtTableSchema.Rows[x]["datatype"].ToString().Trim());
							break;
					}
									
				}
			}
			catch (Exception e)
			{
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:ConfigureDataAdapterInsertCommand  \n" + 
					"Err Msg - " + e.Message.ToString().Trim(),
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				this.m_intError=-1;
			}

		}
/// <summary>
/// create the update command for the data adapter. 
/// </summary>
/// <param name="p_conn">SQLite connection object</param>
/// <param name="p_da">SQLite dataadapter object</param>
/// <param name="p_trans">SQLite transaction object</param>
/// <param name="p_strSQL">select sql statement to get the update field data types</param>
/// <param name="p_strSQLUniqueRecordFields">select SQL statement listing fields used for a records unique id are queried and their field types obtained and added to the dataadapter updates parameters list</param>
/// <param name="p_strTable">table name to be updated</param>
		public void ConfigureDataAdapterUpdateCommand(System.Data.SQLite.SQLiteConnection p_conn, 
			System.Data.SQLite.SQLiteDataAdapter p_da,
			System.Data.SQLite.SQLiteTransaction p_trans,
			string p_strSQL,string p_strSQLUniqueRecordFields,string p_strTable)
		{

			this.m_intError=0;
			System.Data.DataTable p_dtTableSchema = this.getTableSchema(p_conn,p_trans, p_strSQL);
			System.Data.DataTable p_dtTableSchema2 = new DataTable();
			if (this.m_intError !=0) return;
			string strField = "";
			string strValue = "";
			string strSQL="";
			int x;
			try
			{
				//Build the plot update sql
				for (x=0; x<=p_dtTableSchema.Rows.Count-1;x++)
				{
					strField = p_dtTableSchema.Rows[x]["columnname"].ToString().Trim();
					if (strValue.Trim().Length == 0)
					{
						strValue = strField + "=?";
					}
					else
					{	
						strValue += "," + strField + "=?";
					}
				}
				
				strSQL = 
					"UPDATE " + p_strTable + " SET "  +  strValue;

				//get the unique record id
				if (p_strSQLUniqueRecordFields.Trim().Length > 0)
				{
					strValue="";
					p_dtTableSchema2 = this.getTableSchema(p_conn,p_trans,p_strSQLUniqueRecordFields);
					if (this.m_intError !=0) return;
					//build the where condition
					for (x=0; x<=p_dtTableSchema2.Rows.Count-1;x++)
					{
						strField = p_dtTableSchema2.Rows[x]["columnname"].ToString().Trim();
						if (strValue.Trim().Length == 0)
						{
							strValue = strField + "=?";
						}
						else
						{	
							strValue += " AND " + strField + "=?";
						}
					}
					strSQL += " WHERE " + strValue;
				}


				//create an insert command 
				p_da.UpdateCommand = p_conn.CreateCommand();
				//bind the transaction object to the insert command
				p_da.UpdateCommand.Transaction = p_trans;
				p_da.UpdateCommand.CommandText = strSQL;

				//copy the table schema records containing update fields info to a new table
                System.Data.DataTable p_dt = p_dtTableSchema.Copy();

				//define field datatypes for the data adapter
				for (;;)
				{
					for (x=0; x<=p_dt.Rows.Count-1;x++)
					{
						strField=p_dt.Rows[x]["columnname"].ToString().Trim();
						switch (p_dt.Rows[x]["datatype"].ToString().Trim())
						{
							case "System.String" :
								p_da.UpdateCommand.Parameters.Add
									(strField, 
									System.Data.DbType.String,
									0,
									strField);
								break;
							case "System.Double":
								p_da.UpdateCommand.Parameters.Add
									(strField, 
									System.Data.DbType.Double,
									0,
									strField);
								break;
							//case "System.Boolean":
							//	p_da.UpdateCommand.Parameters.Add
							//		(strField, 
							//		System.Data.DbType.,
							//		0,
							//		strField);
							//	break;
							case "System.DateTime":
								p_da.UpdateCommand.Parameters.Add
									(strField, 
									System.Data.DbType.DateTime,
									0,
									strField);
								break;
							case "System.Decimal":
								p_da.UpdateCommand.Parameters.Add
									(strField, 
									System.Data.DbType.Decimal,
									0,
									strField);
								break;
							case "System.Int16":
								p_da.UpdateCommand.Parameters.Add
									(strField, 
									System.Data.DbType.Int16,
									0,
									strField);
								break;
							case "System.Int32":
								p_da.UpdateCommand.Parameters.Add
									(strField, 
									System.Data.DbType.Int32,
									0,
									strField);
								break;
							case "System.Int64":
								p_da.UpdateCommand.Parameters.Add
									(strField, 
									System.Data.DbType.Int64,
									0,
									strField);
								break;
							case "System.SByte":
								p_da.UpdateCommand.Parameters.Add
									(strField, 
									System.Data.DbType.SByte,
									0,
									strField);
								break;
							case "System.Byte":
								p_da.UpdateCommand.Parameters.Add
									(strField, 
									System.Data.DbType.Byte,
									0,
									strField);
								break;
							case "System.Single":
								p_da.UpdateCommand.Parameters.Add
									(strField, 
									System.Data.DbType.Single,
									0,
									strField);
								break;
							default:
								MessageBox.Show("Could Not Set Data Adapter Parameter For DataType " + p_dt.Rows[x]["datatype"].ToString().Trim());
								break;
						}
									
					}
					if (p_strSQLUniqueRecordFields.Trim().Length > 0)
					{
						//clear the data table of all its records
						p_dt.Clear();
						//copy the table schema records containing where clause fields info to a new table
						p_dt = p_dtTableSchema2.Copy();
						p_strSQLUniqueRecordFields = "";
					}
					else
					{
						break;
					}
				}
			}
			catch (Exception e)
			{
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:ConfigureDataAdapterUpdateCommand  \n" + 
					"Err Msg - " + e.Message.ToString().Trim(),
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				this.m_intError=-1;
			}
		}

		/// <summary>
		/// create the delete command for the data adapter. 
		/// </summary>
		/// <param name="p_conn">oledb connection object</param>
		/// <param name="p_da">oledb dataadapter object</param>
		/// <param name="p_trans">oledb transaction object</param>
		/// <param name="p_strSQLUniqueRecordFields">select SQL statement listing fields used for a records unique id are queried and their field types obtained and added to the dataadapter delete command parameters list</param>
		/// <param name="p_strTable">table name to be updated</param>
		public void ConfigureDataAdapterDeleteCommand(System.Data.SQLite.SQLiteConnection p_conn, 
			System.Data.SQLite.SQLiteDataAdapter p_da,
			System.Data.SQLite.SQLiteTransaction p_trans,
			string p_strSQLUniqueRecordFields,string p_strTable)
		{

			this.m_intError=0;
			System.Data.DataTable p_dt = this.getTableSchema(p_conn,p_trans, p_strSQLUniqueRecordFields);
			if (this.m_intError !=0) return;
			string strField = "";
			string strValue = "";
			string strSQL="";
			int x;
			try
			{
				strSQL = "DELETE FROM " + p_strTable + " ";
				//build the where condition
				for (x=0; x<=p_dt.Rows.Count-1;x++)
				{
					strField = p_dt.Rows[x]["columnname"].ToString().Trim();
					if (strValue.Trim().Length == 0)
					{
						strValue = strField + "=?";
					}
					else
					{	
						strValue += " AND " + strField + "=?";
					}
				}
				strSQL += " WHERE " + strValue;
				


				//create an insert command 
				p_da.DeleteCommand = p_conn.CreateCommand();
				//bind the transaction object to the insert command
				p_da.DeleteCommand.Transaction = p_trans;
				p_da.DeleteCommand.CommandText = strSQL;

				

				//define field datatypes for the data adapter
				
				
				for (x=0; x<=p_dt.Rows.Count-1;x++)
				{
					strField=p_dt.Rows[x]["columnname"].ToString().Trim();
					switch (p_dt.Rows[x]["datatype"].ToString().Trim())
					{
						case "System.String" :
							p_da.DeleteCommand.Parameters.Add
								(strField, 
								System.Data.DbType.String,
								0,
								strField);
							break;
						case "System.Double":
							p_da.DeleteCommand.Parameters.Add
								(strField, 
								System.Data.DbType.Double,
								0,
								strField);
							break;
						//case "System.Boolean":
						//	p_da.DeleteCommand.Parameters.Add
						//		(strField, 
						//		System.Data.DbType.Boolean,
						//		0,
						//		strField);
						//	break;
						case "System.DateTime":
							p_da.DeleteCommand.Parameters.Add
								(strField, 
								System.Data.DbType.DateTime,
								0,
								strField);
							break;
						case "System.Decimal":
							p_da.DeleteCommand.Parameters.Add
								(strField, 
								System.Data.DbType.Double,
								0,
								strField);
							break;
						case "System.Int16":
							p_da.DeleteCommand.Parameters.Add
								(strField, 
								System.Data.DbType.Int16,
								0,
								strField);
							break;
						case "System.Int32":
							p_da.DeleteCommand.Parameters.Add
								(strField, 
								System.Data.DbType.Int32,
								0,
								strField);
							break;
						case "System.Int64":
							p_da.DeleteCommand.Parameters.Add
								(strField, 
								System.Data.DbType.Int64,
								0,
								strField);
							break;
						case "System.SByte":
							p_da.DeleteCommand.Parameters.Add
								(strField, 
								System.Data.DbType.SByte,
								0,
								strField);
							break;
						case "System.Byte":
							p_da.DeleteCommand.Parameters.Add
								(strField, 
								System.Data.DbType.Byte,
								0,
								strField);
							break;
						case "System.Single":
							p_da.DeleteCommand.Parameters.Add
								(strField, 
								System.Data.DbType.Single,
								0,
								strField);
							break;
						default:
							MessageBox.Show("Could Not Set Data Adapter Parameter For DataType " + p_dt.Rows[x]["datatype"].ToString().Trim());
							break;
					}
									
				}
			
					
				
			}
			catch (Exception e)
			{
				if (_bDisplayErrors)
				MessageBox.Show("!!Error!! \n" + 
					"Module - SQLite:ConfigureDataAdapterUpdateCommand  \n" + 
					"Err Msg - " + e.Message.ToString().Trim(),
                    "FIA Biosum", System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation);
				this.m_intError=-1;
			}
		}
		public void ConfigureSqlForMSAccessUsage(ref string[] p_strColumns, string[] p_strDataTypes)
		{

		}
        /// <summary>
        /// Dispose of dataadapter object and reconfigure a new dataadapter
        /// </summary>
        /// <param name="TABLE_INDEX">the index within the dataadapter array object m_DataAdapterArray</param>
        /// <param name="p_strTableName">table name within the schema</param>
        /// <param name="p_strColumns">Comma delimited list of all the columns to be configured by the dataadapter including primary key columns.
        ///Can be a subset of all the columns in the table. VALID VALUES: * or comma-delimited list </param>
        /// <param name="p_strPrimaryKey">Comma-delimited list of the primary key(s)</param>
        public void InitializeDataAdapterArrayItem(int TABLE_INDEX, string p_strTableName, string p_strColumns, string p_strPrimaryKeyColumns)
        {
           

            if (m_DataAdapterArray[TABLE_INDEX] != null)
            {
                if (m_DataAdapterArray[TABLE_INDEX].SelectCommand != null)
                {
                   // m_DataAdapterArray[TABLE_INDEX].SelectCommand.Transaction.Dispose();
                    m_DataAdapterArray[TABLE_INDEX].SelectCommand.Dispose();
                }
                if (m_DataAdapterArray[TABLE_INDEX].UpdateCommand != null)
                {
                    //m_DataAdapterArray[TABLE_INDEX].UpdateCommand.Transaction.Dispose();
                    m_DataAdapterArray[TABLE_INDEX].UpdateCommand.Dispose();
                }
                if (m_DataAdapterArray[TABLE_INDEX].DeleteCommand != null)
                {
                    //m_DataAdapterArray[TABLE_INDEX].DeleteCommand.Transaction.Dispose();
                    m_DataAdapterArray[TABLE_INDEX].DeleteCommand.Dispose();
                }
                if (m_DataAdapterArray[TABLE_INDEX].InsertCommand != null)
                {
                    //m_DataAdapterArray[TABLE_INDEX].InsertCommand.Transaction.Dispose();
                    m_DataAdapterArray[TABLE_INDEX].InsertCommand.Dispose();
                }
                m_DataAdapterArray[TABLE_INDEX].Dispose();
            }
            for (int x = 0; x <= m_DataSet.Tables.Count - 1; x++)
            {
                if (m_DataSet.Tables[x].TableName.ToUpper().Trim() == p_strTableName.ToUpper().Trim())
                {
                    m_DataSet.Tables[p_strTableName].Clear();
                    m_DataSet.Tables[p_strTableName].Dispose();
                    break;
                }
            }
            m_DataAdapterArray[TABLE_INDEX] = new SQLiteDataAdapter();
            m_strSQL = "SELECT " + p_strColumns + " FROM " + p_strTableName;
            InitializeOleDbTransactionCommands(m_DataAdapterArray[TABLE_INDEX], p_strTableName, p_strColumns, p_strPrimaryKeyColumns);
            m_strSQL = "SELECT " + p_strColumns + " FROM " + p_strTableName;
            m_Command = m_Connection.CreateCommand();
            m_Command.CommandText = m_strSQL;
            m_DataAdapterArray[TABLE_INDEX].SelectCommand = m_Command;
            m_DataAdapterArray[TABLE_INDEX].SelectCommand.Transaction = m_Transaction;
            m_DataAdapterArray[TABLE_INDEX].Fill(this.m_DataSet, p_strTableName);
            m_DataSet.Tables[p_strTableName].PrimaryKey = new System.Data.DataColumn[] { this.m_DataSet.Tables[p_strTableName].Columns[p_strPrimaryKeyColumns] };
            m_DataAdapterArrayTableName[TABLE_INDEX] = p_strTableName;
        }
        public int getDataAdaperArrayItemIndex(string p_strTableName)
        {
            int index = -1;
            if (m_DataAdapterArrayTableName == null) return -1;
            if (m_DataAdapterArrayTableName.Length == 0) return -1;
            if (m_DataAdapterArrayTableName[0] == null) return -1;
            for (index = 0; index <= m_DataAdapterArrayTableName.Length - 1; index++)
            {
                if (m_DataAdapterArrayTableName[index] != null)
                {
                    if (m_DataAdapterArrayTableName[index].Trim().ToUpper() == p_strTableName.Trim().ToUpper()) break;
                }
            }
            if (index > m_DataAdapterArrayTableName.Length - 1) index = -1;
            return index;
        }
        /// <summary>
        /// Reset the tranaction object (m_Transaction) to begin a new transaction and assign
        /// the transaction object (m_Transaction) to each
        /// data adapter object contained in the array (m_DataAdapterArray). 
        /// </summary>
        public void ResetTransactionObjectToDataAdapterArray()
        {
            m_Transaction = m_Connection.BeginTransaction();


            foreach (SQLiteDataAdapter da in m_DataAdapterArray)
            {
                if (da != null)
                {
                    da.InsertCommand.Transaction = m_Transaction;
                    da.DeleteCommand.Transaction = m_Transaction;
                    da.UpdateCommand.Transaction = m_Transaction;
                }


            }

        }
        /// <summary>
        /// Reset the transaction object (m_Transaction) and 
        /// begin a new transaction.  Assign the transaction object
        /// (m_Transaction) to the data adapter object (m_DataAdapter)
        /// </summary>
        public void ResetTransactionObjectToDataAdapter()
        {
            m_Transaction = m_Connection.BeginTransaction();

            m_DataAdapter.InsertCommand.Transaction = m_Transaction;
            m_DataAdapter.DeleteCommand.Transaction = m_Transaction;
            m_DataAdapter.UpdateCommand.Transaction = m_Transaction;
            

        }
        public void InitializeDataAdapterArray()
        {
            InitializeDataAdapterArray(DataAdapterTableCount);
        }
        /// <summary>
        /// Size and create dataadapter array (m_DataAdapterArray), dispose of each existing dataadapter object in the array (m_DataAdapterArray,
        /// and instantiate each dataadapter object in the array (m_DataAdapterArray).
        /// </summary>
        /// <param name="p_intTableCount">Size of the array</param>
        public void InitializeDataAdapterArray(int p_intTableCount)
        {


            int x = 0;

            if (m_DataAdapterArray != null && p_intTableCount != m_DataAdapterArray.Count())
            {
                for (x=0;x<=m_DataAdapterArray.Count()-1;x++)
                {
                    if (m_DataAdapterArray[x] != null)
                    {
                        if (m_DataAdapterArray[x].SelectCommand != null)
                        {
                            //m_DataAdapterArray[x].SelectCommand.Transaction.Dispose();
                            m_DataAdapterArray[x].SelectCommand.Dispose();
                        }
                        if (m_DataAdapterArray[x].UpdateCommand != null)
                        {
                            // m_DataAdapterArray[x].UpdateCommand.Transaction.Dispose();
                            m_DataAdapterArray[x].UpdateCommand.Dispose();
                        }
                        if (m_DataAdapterArray[x].DeleteCommand != null)
                        {
                            //m_DataAdapterArray[x].DeleteCommand.Transaction.Dispose();
                            m_DataAdapterArray[x].DeleteCommand.Dispose();
                        }
                        if (m_DataAdapterArray[x].InsertCommand != null)
                        {
                            //m_DataAdapterArray[x].InsertCommand.Transaction.Dispose();
                            m_DataAdapterArray[x].InsertCommand.Dispose();
                        }
                        m_DataAdapterArray[x].Dispose();
                        m_DataAdapterArray[x] = null;
                    }
                }
                m_DataAdapterArray = null;
                m_DataAdapterArrayTableName = null;   
            }
            if (m_DataAdapterArray != null)
            {
                DataAdapterTableCount = p_intTableCount;
                for (x = 0; x <= p_intTableCount - 1; x++)
                {
                    
                    if (x<=m_DataAdapterArray.Count() && m_DataAdapterArray[x] != null)
                    {
                        if (m_DataAdapterArray[x].SelectCommand != null)
                        {
                            //m_DataAdapterArray[x].SelectCommand.Transaction.Dispose();
                            m_DataAdapterArray[x].SelectCommand.Dispose();
                        }
                        if (m_DataAdapterArray[x].UpdateCommand != null)
                        {
                           // m_DataAdapterArray[x].UpdateCommand.Transaction.Dispose();
                            m_DataAdapterArray[x].UpdateCommand.Dispose();
                        }
                        if (m_DataAdapterArray[x].DeleteCommand != null)
                        {
                            //m_DataAdapterArray[x].DeleteCommand.Transaction.Dispose();
                            m_DataAdapterArray[x].DeleteCommand.Dispose();
                        }
                        if (m_DataAdapterArray[x].InsertCommand != null)
                        {
                            //m_DataAdapterArray[x].InsertCommand.Transaction.Dispose();
                            m_DataAdapterArray[x].InsertCommand.Dispose();
                        }
                        m_DataAdapterArray[x].Dispose();
                        m_DataAdapterArray[x] = null;
                    }
                    m_DataAdapterArray[x] = new SQLiteDataAdapter();
                    m_DataAdapterArrayTableName[x] = "";
                }
            }
            else
            {
                if (m_DataAdapterArray == null)
                {
                    m_DataAdapterArray = new SQLiteDataAdapter[p_intTableCount];
                    m_DataAdapterArrayTableName = new string[p_intTableCount];
                    for (x = 0; x <= m_DataAdapterArray.Count() - 1; x++)
                    {
                        m_DataAdapterArray[x] = null;
                        m_DataAdapterArrayTableName[x] = "";

                    }
                }
            }
           

        }
        /// <summary>
        /// Dispose of dataadapter object (m_DataAdapter) and reconfigure a new dataadapter (m_DataAdapter)
        /// </summary>
        /// <param name="p_strTableName">table name within the schema</param>
        /// <param name="p_strColumns">Comma delimited list of all the columns to be configured by the dataadapter including primary key columns.
        ///Can be a subset of all the columns in the table. VALID VALUES: * or comma-delimited list </param>
        /// <param name="p_strPrimaryKey">Comma-delimited list of the primary key(s)</param>
        public void InitializeDataAdapter(string p_strTableName, string p_strColumns, string p_strPrimaryKeyColumns,int p_intMAXRecords,string p_strWhereCondition)
        {
            if (m_DataAdapter != null)
            {
                if (m_DataAdapter.SelectCommand != null)
                {
                    m_DataAdapter.SelectCommand.Dispose();
                }
                if (m_DataAdapter.UpdateCommand != null)
                {
                    m_DataAdapter.UpdateCommand.Dispose();
                }
                if (m_DataAdapter.DeleteCommand != null)
                {
                    m_DataAdapter.DeleteCommand.Dispose();
                }
                if (m_DataAdapter.InsertCommand != null)
                {
                    m_DataAdapter.InsertCommand.Dispose();
                }
                m_DataAdapter.Dispose();
            }
            //if (m_Transaction != null) m_Transaction.Dispose();
            for (int x = 0; x <= m_DataSet.Tables.Count - 1; x++)
            {
                if (m_DataSet.Tables[x].TableName.ToUpper().Trim() == p_strTableName.ToUpper().Trim())
                {
                    m_DataSet.Tables[p_strTableName].Clear();
                    m_DataSet.Tables[p_strTableName].Dispose();
                    break;
                }
            }
            this.m_DataAdapter = new SQLiteDataAdapter();
            m_strSQL = "SELECT " + p_strColumns + " FROM " + p_strTableName;
            InitializeOleDbTransactionCommands(p_strTableName, p_strColumns, p_strPrimaryKeyColumns);
            if (p_strWhereCondition != null && p_strWhereCondition.Trim().Length > 0)
                m_strSQL = "SELECT " + p_strColumns + " FROM " + p_strTableName + " WHERE " + p_strWhereCondition;
            else
                m_strSQL = "SELECT " + p_strColumns + " FROM " + p_strTableName;
            m_Command = m_Connection.CreateCommand();
            m_Command.CommandText = m_strSQL;
            m_DataAdapter.SelectCommand = m_Command;
            m_DataAdapter.SelectCommand.Transaction = m_Transaction;
            if (p_intMAXRecords > 0)
            {
                this.m_DataAdapter.Fill(this.m_DataSet, 0, p_intMAXRecords, p_strTableName);
            }
            else
            {
                this.m_DataAdapter.Fill(this.m_DataSet, p_strTableName);
            }
            m_DataSet.Tables[p_strTableName].PrimaryKey = new System.Data.DataColumn[] { this.m_DataSet.Tables[p_strTableName].Columns[p_strPrimaryKeyColumns] };
        }
        /// <summary>
        /// identify primary key columns and configure dataadaper for INSERT,UPDATE, and DELETE.
        /// </summary>
        /// <param name="p_DataAdapter">dataadapter object</param>
        /// <param name="p_strTableName">table name within the schema</param>
        /// <param name="p_strColumns">Comma delimited list of all the columns to be configured by the dataadapter.
        ///Can be a subset of all the columns in the table. VALID VALUES: * or comma-delimited list </param>
        /// <param name="p_strPrimaryKey">Comma-delimited list of the primary key(s)</param>
        public void InitializeOleDbTransactionCommands(SQLiteDataAdapter p_DataAdapter, string p_strTableName,string p_strColumns, string p_strPrimaryKey)
        {
            string strDelimiter = ",";
            string strNonPrimaryKeyColumns = "";
            string[] strColumnArray = getFieldNamesArray(m_Connection, "select " + p_strColumns + " from " + p_strTableName);
            string[] strPrimaryKeyArray;
            //check if more than one primary key column
            if (p_strPrimaryKey.IndexOf(",", 0) > 0)
                strPrimaryKeyArray = p_strPrimaryKey.Split(strDelimiter.ToCharArray());
            else
                strPrimaryKeyArray = new string[1]; strPrimaryKeyArray[0] = p_strPrimaryKey;

            for (int x = 0; x <= strColumnArray.Length - 1; x++)
            {
                if (strColumnArray[x] != null && strColumnArray[x].Trim().Length > 0)
                {
                    //make sure column is not part of the primary key
                    int COUNT = strPrimaryKeyArray.Where(pk => pk.Trim().ToUpper() == strColumnArray[x].Trim().ToUpper()).Count();
                    if (COUNT == 0)
                        strNonPrimaryKeyColumns = strNonPrimaryKeyColumns + strColumnArray[x].Trim() + ",";
                }
            }
            //remove last comma
            if (strNonPrimaryKeyColumns.Length > 0) strNonPrimaryKeyColumns = strNonPrimaryKeyColumns.Substring(0, strNonPrimaryKeyColumns.Length - 1);

            this.m_strSQL = "select " + p_strColumns + " from " + p_strTableName;
            //initialize the transaction object with the connection
            //this.m_Transaction = this.m_Connection.BeginTransaction();
            this.ConfigureDataAdapterInsertCommand(this.m_Connection,
                p_DataAdapter,
                this.m_Transaction,
                this.m_strSQL,
                p_strTableName);

            this.m_strSQL = "select " + strNonPrimaryKeyColumns + " from " + p_strTableName;
            this.ConfigureDataAdapterUpdateCommand(this.m_Connection,
                p_DataAdapter,
                this.m_Transaction,
                this.m_strSQL, "select " + p_strPrimaryKey + " FROM " + p_strTableName,
                p_strTableName);

            this.m_strSQL = "select " + strNonPrimaryKeyColumns + " from " + p_strTableName;
            this.ConfigureDataAdapterDeleteCommand(this.m_Connection,
                p_DataAdapter,
                this.m_Transaction,
                "select " + p_strPrimaryKey + " FROM " + p_strTableName,
                p_strTableName);




        }
        /// <summary>
        /// identify primary key columns and configure dataadaper (m_DataAdapter) for INSERT,UPDATE, and DELETE.
        /// </summary>
        /// <param name="p_strTableName">table name within the schema</param>
        /// <param name="p_strColumns">Comma delimited list of all the columns to be configured by the dataadapter.
        ///Can be a subset of all the columns in the table. VALID VALUES: * or comma-delimited list </param>
        /// <param name="p_strPrimaryKey">Comma-delimited list of the primary key(s)</param>
        public void InitializeOleDbTransactionCommands(string p_strTableName, string p_strColumns, string p_strPrimaryKey)
        {
            string strDelimiter = ",";
            string strNonPrimaryKeyColumns = "";
            string[] strColumnArray = getFieldNamesArray(m_Connection, "select " + p_strColumns + " from " + p_strTableName);
            string[] strPrimaryKeyArray;
            //check if more than one primary key column
            if (p_strPrimaryKey.IndexOf(",", 0) > 0)
                strPrimaryKeyArray = p_strPrimaryKey.Split(strDelimiter.ToCharArray());
            else
                strPrimaryKeyArray = new string[1]; strPrimaryKeyArray[0] = p_strPrimaryKey;

            for (int x = 0; x <= strColumnArray.Length - 1; x++)
            {
                if (strColumnArray[x] != null && strColumnArray[x].Trim().Length > 0)
                {
                    //make sure column is not part of the primary key
                    int COUNT = strPrimaryKeyArray.Where(pk => pk.Trim().ToUpper() == strColumnArray[x].Trim().ToUpper()).Count();
                    if (COUNT == 0)
                        strNonPrimaryKeyColumns = strNonPrimaryKeyColumns + strColumnArray[x].Trim() + ",";
                }
            }
            //remove last comma
            if (strNonPrimaryKeyColumns.Length > 0) strNonPrimaryKeyColumns = strNonPrimaryKeyColumns.Substring(0, strNonPrimaryKeyColumns.Length - 1);

            m_strSQL = "select " + p_strColumns + " from " + p_strTableName;
            //initialize the transaction object with the connection
            //m_Transaction = m_Connection.BeginTransaction();
            ConfigureDataAdapterInsertCommand(m_Connection,
                m_DataAdapter,
                m_Transaction,
                m_strSQL,
                p_strTableName);

            m_strSQL = "select " + strNonPrimaryKeyColumns + " from " + p_strTableName;
            ConfigureDataAdapterUpdateCommand(m_Connection,
                m_DataAdapter,
                m_Transaction,
                m_strSQL, "select " + p_strPrimaryKey + " FROM " + p_strTableName,
                p_strTableName);

            m_strSQL = "select " + strNonPrimaryKeyColumns + " from " + p_strTableName;
            ConfigureDataAdapterDeleteCommand(m_Connection,
                m_DataAdapter,
                m_Transaction,
                "select " + p_strPrimaryKey + " FROM " + p_strTableName,
                p_strTableName);


        }
		public bool TableExist(System.Data.SQLite.SQLiteConnection p_conn,string p_strTable)
		{
            string strSQL = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND TRIM(UPPER(name))='" + p_strTable.Trim().ToUpper() + "'";
            int intCount = (int)getRecordCount(p_conn, strSQL, "temp");
            if (intCount > 0) return true;
            else return false;
		}
        /// <summary>
        /// Check if the column exists in the designated table
        /// </summary>
        /// <param name="p_oConn"></param>
        /// <param name="p_strTableName"></param>
        /// <param name="p_strColumnName"></param>
        /// <returns></returns>
        public bool ColumnExist(System.Data.SQLite.SQLiteConnection p_oConn, string p_strTableName, string p_strColumnName)
        {
            bool bFound = false;
            if (TableExist(p_oConn, p_strTableName))
            {
                string[] strArray = this.getFieldNamesArray(p_oConn, "SELECT * FROM " + p_strTableName);
                if (strArray != null)
                {
                    for (int x = 0; x <= strArray.Length - 1; x++)
                    {
                        if (p_strColumnName.Trim().ToUpper() == strArray[x].Trim().ToUpper())
                        {
                            bFound = true;
                            break;
                        }
                    }
                }
            }
            return bFound;
        }
        /// <summary>
        /// Return an array of field names after executing the SELECT SQL 
        /// </summary>
        /// <param name="p_oConn"></param>
        /// <param name="p_strSql"></param>
        /// <returns></returns>
        public string[] getFieldNamesArray(System.Data.SQLite.SQLiteConnection p_oConn, string p_strSql)
        {
            this.m_intError = 0;
            string strList = getFieldNames(p_oConn, p_strSql);
            if (strList.Trim().Length == 0) return null;

            string strDelimiter = ",";
            string[] strArray = strList.Split(strDelimiter.ToCharArray());
            return strArray;


        }
        public void CloseAndDisposeConnection(System.Data.SQLite.SQLiteConnection p_Connection, bool p_bClearPool)
        {
            try
            {
                
                if (p_Connection.State != ConnectionState.Closed)
                {
                    if (m_DataReader != null) m_DataReader.Dispose();

                    if (m_Command != null) m_Command.Dispose();

                    if (m_DataAdapter != null)
                    {
                        if (m_DataAdapter.SelectCommand != null)
                        {
                            m_DataAdapter.SelectCommand.Dispose();
                        }
                        if (m_DataAdapter.UpdateCommand != null)
                        {
                            m_DataAdapter.UpdateCommand.Dispose();
                        }
                        if (m_DataAdapter.DeleteCommand != null)
                        {
                            m_DataAdapter.DeleteCommand.Dispose();
                        }
                        if (m_DataAdapter.InsertCommand != null)
                        {
                            m_DataAdapter.InsertCommand.Dispose();
                        }
                    }

                }
                while (p_Connection.State != System.Data.ConnectionState.Closed)
                {

                    p_Connection.Close();
                    System.Threading.Thread.Sleep(1000);



                }
                if (p_Connection.State == ConnectionState.Closed)
                {
                    if (p_bClearPool) SQLiteConnection.ClearPool(p_Connection);
                    p_Connection.Dispose();
                    p_Connection = null;

                }
            }
            catch (Exception e)
            {
                m_strError = e.Message;
                m_intError = -1;
            }
        }

		public void CloseConnection(System.Data.SQLite.SQLiteConnection p_conn)
		{
            
			try
			{
				p_conn.Close();
			}
			catch
			{
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_bUseDataAdapterArray"></param>
        /// <param name="TABLECOUNT"></param>
        /// <param name="p_strFileName">Fully qualified path and filename</param>
        public void OpenConnection(bool p_bUseDataAdapterArray, int TABLECOUNT, string p_strFileName,string p_strDataSetName)
        {
            try
            {


                if (ConnectionDisposed == false && m_Connection != null && m_Connection.State != System.Data.ConnectionState.Closed)
                {
                    if (m_DataReader != null) m_DataReader.Dispose();

                    if (m_Command != null) m_Command.Dispose();

                    //if (m_Transaction != null) m_Transaction.Dispose();

                    if (m_DataAdapter != null)
                    {
                        if (m_DataAdapter.SelectCommand != null)
                        {
                            m_DataAdapter.SelectCommand.Dispose();
                        }
                        if (m_DataAdapter.UpdateCommand != null)
                        {
                            m_DataAdapter.UpdateCommand.Dispose();
                        }
                        if (m_DataAdapter.DeleteCommand != null)
                        {
                            m_DataAdapter.DeleteCommand.Dispose();
                        }
                        if (m_DataAdapter.InsertCommand != null)
                        {
                            m_DataAdapter.InsertCommand.Dispose();
                        }
                    }
                    CloseConnection(m_Connection);
                }


                System.Threading.Thread.Sleep(2000); //sleep 5 seconds



                if (ConnectionDisposed == false && m_Connection != null) m_Connection.Dispose();


                OpenConnection("Data Source=" + p_strFileName + ";Pooling=True;Max Pool Size=10000;");
            }
            catch (System.Data.SQLite.SQLiteException errSQLite)
            {
                m_strError = errSQLite.Message;
                m_intError = -1;
            }
            catch (Exception err)
            {
                m_strError = err.Message;
                m_intError = -1;
            }
            finally
            {
                if (m_intError != 0 || m_intError != 0)
                {
                    CloseConnection(m_Connection);
                }
                else
                {
                    if (m_DataSet != null)
                    {
                        m_DataSet.Tables.Clear();
                        m_DataSet.Dispose();
                    }

                    this.m_DataSet = new System.Data.DataSet(p_strDataSetName);

                    if (p_bUseDataAdapterArray && TABLECOUNT > 0)
                        InitializeDataAdapterArray(TABLECOUNT);
                }
                DataAdapterTableCount = TABLECOUNT;

            }
        }
        
        /// <summary>
        /// See if there are other values not equal to the specified value
        /// Error return values: -100 = Table Does Not Exist; -200 Column does Not Exist
        /// </summary>
        /// <param name="p_oConn"></param>
        /// <param name="p_strTableName">Table to search</param>
        /// <param name="p_strColumnName">Column rows to search</param>
        /// <param name="p_strValue">search for values not equal to this specified value</param>
        /// <param name="p_bNumericDataType"></param>
        /// <returns></returns>
        public bool ValuesExistNotEqualToTargetValue(System.Data.SQLite.SQLiteConnection p_oConn,
                                                          string p_strTableName,
                                                          string p_strColumnName,
                                                          string p_strValue,
                                                          bool p_bNumericDataType)
        {
            m_intError = 0;
            m_strError = "";
           
            bool bFound = false;
            string strSql = "";

            if (TableExist(p_oConn, p_strTableName))
            {
                if (ColumnExist(p_oConn, p_strTableName, p_strColumnName))
                {
                    strSql = "SELECT COUNT(*) FROM (SELECT TOP 1 * FROM " + p_strTableName.Trim() + " WHERE ";
                    if (p_bNumericDataType)
                    {
                        strSql = strSql + p_strColumnName.Trim() + " <> " + p_strValue.Trim() + ")";

                    }
                    else
                    {
                        strSql = strSql + "TRIM(" + p_strColumnName.Trim() + ") <> '" + p_strValue.Trim() + "')";
                    }
                    if (getRecordCount(p_oConn, strSql, p_strTableName.Trim()) > 0)
                    {
                        bFound = true;
                    }
                }
                else
                {
                    m_intError = this.ErrorCodeColumnNotFound;
                    m_strError = p_strTableName + "." + p_strColumnName + " does not exist";
                }
            }
            else
            {
                m_intError = this.ErrorCodeTableNotFound;
                m_strError = p_strTableName + " does not exist";
            }

            return bFound;
        }
        /// <summary>
        /// See if the target value exists 
        /// Error return values: -100 = Table Does Not Exist; -200 Column does Not Exist
        /// </summary>
        /// <param name="p_oAdo"></param>
        /// <param name="p_oConn"></param>
        /// <param name="p_strTableName"></param>
        /// <param name="p_intRxYear"></param>
        /// <returns></returns>
        public bool ValuesExistEqualToTargetValue(System.Data.SQLite.SQLiteConnection p_oConn,
                                                  string p_strTableName,
                                                  string p_strColumnName,
                                                  string p_strValue,
                                                  bool p_bNumericDataType)
        {
           


            m_intError = 0;
            m_strError = "";

            bool bFound = false;
            string strSql = "";

            if (TableExist(p_oConn, p_strTableName))
            {
                if (ColumnExist(p_oConn, p_strTableName, p_strColumnName))
                {
                    strSql = "SELECT COUNT(*) FROM (SELECT TOP 1 * FROM " + p_strTableName.Trim() + " WHERE ";
                    if (p_bNumericDataType)
                    {
                        strSql = strSql + p_strColumnName.Trim() + " = " + p_strValue.Trim() + ")";

                    }
                    else
                    {
                        strSql = strSql + "TRIM(" + p_strColumnName.Trim() + ") = '" + p_strValue.Trim() + "')";
                    }
                    if (getRecordCount(p_oConn, strSql, p_strTableName.Trim()) > 0)
                    {
                        bFound = true;
                    }
                }
                else
                {
                    m_intError = this.ErrorCodeColumnNotFound;
                    m_strError = p_strTableName + "." + p_strColumnName + " does not exist";
                }
            }
            else
            {
                m_intError = this.ErrorCodeTableNotFound;
                m_strError = p_strTableName + " does not exist";
            }

            return bFound;


        }


        /// <summary>
        /// Alter an MS Access table to add a primary key index
        /// </summary>
        /// <param name="p_oConn"></param>
        /// <param name="p_strTableName"></param>
        /// <param name="p_strIndexName"></param>
        /// <param name="p_strColumnList"></param>
        public void AddPrimaryKey(System.Data.SQLite.SQLiteConnection p_oConn, string p_strTableName, string p_strIndexName, string p_strColumnList)
        {
            this.m_strSQL = "ALTER TABLE " + p_strTableName + " " +
                            "ADD CONSTRAINT " + p_strIndexName + " " +
                            "PRIMARY KEY (" + p_strColumnList + ")";
            this.SqlNonQuery(p_oConn, this.m_strSQL);

        }
        /// <summary>
        /// Alter an MS Access table to add an autonumber data type to a column
        /// </summary>
        /// <param name="p_oConn"></param>
        /// <param name="p_strTableName"></param>
        /// <param name="p_strColumnName"></param>
        public void AddAutoNumber(System.Data.SQLite.SQLiteConnection p_oConn, string p_strTableName, string p_strColumnName)
        {
            this.m_strSQL = "ALTER TABLE " + p_strTableName + " ALTER COLUMN " + p_strColumnName + " AUTOINCREMENT";
            SqlNonQuery(p_oConn, m_strSQL);
        }
        /// <summary>
        /// Create an index
        /// </summary>
        /// <param name="p_oConn"></param>
        /// <param name="p_strTableName"></param>
        /// <param name="p_strIndexName"></param>
        /// <param name="p_strColumnList"></param>
        public void AddIndex(System.Data.SQLite.SQLiteConnection p_oConn, string p_strTableName, string p_strIndexName, string p_strColumnList)
        {
            m_strSQL = "CREATE INDEX " + p_strIndexName + " " +
                       "ON " + p_strTableName + " " +
                       "(" + p_strColumnList + ")";
            SqlNonQuery(p_oConn, m_strSQL);

        }
        public void AddColumn(System.Data.SQLite.SQLiteConnection p_oConn, string p_strTableName, string p_strColumnName, string p_strDataType, string p_strSize)
        {
            if (p_strSize != null && p_strSize.Trim().Length > 0)
            {
                SqlNonQuery(p_oConn, "ALTER TABLE " + p_strTableName + " " +
                                "ADD COLUMN " + p_strColumnName + " " + p_strDataType + " (" + p_strSize + ")");
            }
            else
            {
                SqlNonQuery(p_oConn, "ALTER TABLE " + p_strTableName + " " +
                               "ADD COLUMN " + p_strColumnName + " " + p_strDataType);
            }
        }

        public bool ReconcileTableColumns(System.Data.SQLite.SQLiteConnection p_oDestConn,
            string p_strDestTable,
            System.Data.SQLite.SQLiteConnection p_oSourceConn,
            string p_strSourceTable)
        {
            bool bFound = false;
            bool bModified = false;
            int z, zz;
            System.Data.DataTable oDtSourceSchema = getTableSchema(p_oSourceConn, "SELECT * FROM " + p_strSourceTable);
            System.Data.DataTable oDtDestSchema = getTableSchema(p_oDestConn, "SELECT * FROM " + p_strDestTable);
            for (z = 0; z <= oDtSourceSchema.Rows.Count - 1; z++)
            {
                string strSourceColumnFormat = "";
                string strDestColumnFormat = "";
                if (oDtSourceSchema.Rows[z]["ColumnName"] != System.DBNull.Value)
                {
                    bFound = false;
                    for (zz = 0; zz <= oDtDestSchema.Rows.Count - 1; zz++)
                    {
                        if (oDtDestSchema.Rows[zz]["ColumnName"] != System.DBNull.Value)
                        {
                            if (oDtSourceSchema.Rows[z]["ColumnName"].ToString().Trim().ToUpper() ==
                                oDtDestSchema.Rows[zz]["ColumnName"].ToString().Trim().ToUpper())
                            {
                                strSourceColumnFormat = this.FormatCreateTableSqlFieldItem(oDtSourceSchema.Rows[z]);
                                strDestColumnFormat = this.FormatCreateTableSqlFieldItem(oDtDestSchema.Rows[zz]);
                                if (strSourceColumnFormat.Trim().ToUpper() != strDestColumnFormat.Trim().ToUpper())
                                {
                                    //alter the column to the new specs
                                    this.m_strSQL = "ALTER TABLE " + p_strDestTable + " ALTER COLUMN " + strSourceColumnFormat;
                                    this.SqlNonQuery(p_oDestConn, m_strSQL);
                                    bModified = true;
                                }
                                bFound = true;
                                break;
                            }
                        }
                    }
                    if (!bFound)
                    {
                        //column not found so let's add it
                        strSourceColumnFormat = this.FormatCreateTableSqlFieldItem(oDtSourceSchema.Rows[z]);
                        SqlNonQuery(p_oDestConn, "ALTER TABLE " + p_strDestTable + " " +
                            "ADD COLUMN " + strSourceColumnFormat);
                        bModified = true;
                    }
                }

            }
            return bModified;

        }
        /// <summary>
        /// Return a formatted string used to compile a CREATE TABLE sql command
        /// </summary>
        /// <param name="p_oRow">ADO.NET datarow</param>
        /// <returns></returns>
        public string FormatCreateTableSqlFieldItem(System.Data.DataRow p_oRow)
        {
            string strLine;
            string strColumn = p_oRow["ColumnName"].ToString().Trim();
            string strDataType = p_oRow["DataType"].ToString().Trim();
            string strPrecision = "";
            string strScale = "";
            string strSize = "";

            if (p_oRow["ColumnSize"] != null)
                strSize = Convert.ToString(p_oRow["ColumnSize"]);

            if (p_oRow["NumericPrecision"] != null)
                strPrecision = Convert.ToString(p_oRow["NumericPrecision"]);

            if (p_oRow["NumericScale"] != null)
                strScale = Convert.ToString(p_oRow["NumericScale"]);



            strColumn = "[" + strColumn + "]";


            switch (strDataType)
            {

                case "System.Single":
                    strDataType = "single";
                    break;
                case "System.Double":
                    strDataType = "double";
                    break;
                case "System.Decimal":
                    strDataType = "decimal";
                    break;
                case "System.String":
                    strDataType = "text";
                    break;
                case "System.Int16":
                    strDataType = "short";
                    break;
                case "System.Char":
                    strDataType = "text";
                    break;
                case "System.Int32":
                    strDataType = "integer";
                    break;
                case "System.DateTime":
                    strDataType = "datetime";
                    break;
                case "System.DayOfWeek":
                    break;
                case "System.Int64":
                    break;
                case "System.Byte":
                    strDataType = "byte";
                    break;
                case "System.Boolean":
                    break;



            }

            strLine = strColumn + " " + strDataType;

            if (strSize.Trim().Length > 0 && strDataType == "text")
                if (Convert.ToInt32(strSize) < 256)
                    strLine = strLine + " (" + strSize + ")";
                else
                {
                    strLine = strColumn + " memo";
                }
            else
            {
                if (strDataType == "decimal")
                {
                    if (strPrecision.Trim() == "0")
                        strLine = strColumn + " double";
                    else
                        strLine = strLine + " (" + strPrecision + "," + strScale + ")";
                }


            }
            return strLine;

        }
        /// <summary>
        /// Format SQL command string for these issues:
        /// 1. numeric values to handle MS ACCESS decimal length maximums
        /// 2. MS ACCESS reserved words
        /// </summary>
        /// <param name="p_strColumnName"></param>
        /// <param name="p_strDataType"></param>
        /// <param name="p_strColumnSize"></param>
        /// <param name="p_strNumericPrecision"></param>
        /// <param name="p_strNumericScale"></param>
        /// <param name="p_bReservedWordFormatting"></param>
        /// <returns></returns>
        public string FormatSelectSqlFieldItem(string p_strColumnName, string p_strDataType,
            string p_strColumnSize, string p_strNumericPrecision,
            string p_strNumericScale, bool p_bReservedWordFormatting)
        {
            string strLine = "";
            string strPrecision = "";
            string strScale = "";
            string strSize = "";

            if (p_strColumnSize != null && p_strColumnSize.Trim().Length > 0)
                strSize = p_strColumnSize;

            if (p_strNumericPrecision != null && p_strNumericPrecision.Trim().Length > 0)
                strPrecision = p_strNumericPrecision;

            if (p_strNumericScale != null && p_strNumericScale.Trim().Length > 0)
                strScale = p_strNumericScale;

            if (p_bReservedWordFormatting)
            {
                p_strColumnName = FormatReservedWordColumnName(p_strColumnName);
            }




            switch (p_strDataType)
            {

                case "System.Single":
                    p_strDataType = "single";
                    break;
                case "System.Double":
                    p_strDataType = "double";
                    break;
                case "System.Decimal":
                    p_strDataType = "decimal";
                    break;
                case "System.String":
                    p_strDataType = "text";
                    break;
                case "System.Int16":
                    p_strDataType = "short";
                    break;
                case "System.Char":
                    p_strDataType = "text";
                    break;
                case "System.Int32":
                    p_strDataType = "integer";
                    break;
                case "System.DateTime":
                    p_strDataType = "datetime";
                    break;
                case "System.DayOfWeek":
                    break;
                case "System.Int64":
                    break;
                case "System.Byte":
                    break;
                case "System.Boolean":
                    break;



            }

            strLine = p_strColumnName;

            if (p_strDataType == "decimal")
            {
                if (strPrecision.Trim() == "0")
                    strLine = "ROUND(" + p_strColumnName + ",14) AS " + p_strColumnName;
            }
            else if (p_strDataType == "double")
            {
                strLine = "ROUND(" + p_strColumnName + ",14) AS " + p_strColumnName;
            }


            return strLine;
        }
        public string FormatSelectSqlFieldItem(System.Data.DataRow p_oRow, bool p_bReservedWordFormatting)
        {
            string strLine = "";
            string strColumn = p_oRow["ColumnName"].ToString().Trim();
            string strDataType = p_oRow["DataType"].ToString().Trim();
            string strPrecision = "";
            string strScale = "";
            string strSize = "";

            if (p_oRow["ColumnSize"] != null)
                strSize = Convert.ToString(p_oRow["ColumnSize"]);

            if (p_oRow["NumericPrecision"] != null)
                strPrecision = Convert.ToString(p_oRow["NumericPrecision"]);

            if (p_oRow["NumericScale"] != null)
                strScale = Convert.ToString(p_oRow["NumericScale"]);

            return strLine = FormatSelectSqlFieldItem(strColumn, strDataType, strSize, strPrecision, strScale, p_bReservedWordFormatting);
        }
        /// <summary>
        /// Format reserved words used as column names in SQL expressions.
        /// </summary>
        /// <param name="p_strColumnName"></param>
        /// <returns></returns>
        public string FormatReservedWordColumnName(string p_strColumnName)
        {
            if (p_strColumnName.Trim().ToUpper() == "VALUE" ||
                p_strColumnName.Trim().ToUpper() == "USE" ||
                p_strColumnName.Trim().ToUpper() == "YEAR" ||
                p_strColumnName.Trim().ToUpper() == "DESC" ||
                p_strColumnName.Trim().ToUpper() == "AS")
            {
                p_strColumnName = "`" + p_strColumnName.Trim() + "`";
            }

            return p_strColumnName;

        }
        public string FormatReservedWordsInColumnNameList(string p_strList, string p_strDelimiter)
        {
            string strList = "";
            if (p_strList.Trim().Length == 0) return "";
            string[] strArray = p_strList.Split(p_strDelimiter.ToCharArray());
            for (int x = 0; x <= strArray.Length - 1; x++)
            {
                strList = strList + this.FormatReservedWordColumnName(strArray[x]) + ",";
            }
            strList = strList.Substring(0, strList.Length - 1);
            return strList;
        }
        public bool ColumnExists(System.Data.SQLite.SQLiteConnection p_oConn, string p_strTableName, string p_strColumnName)
        {
            bool bExists = false;
            using (var cmd = new SQLiteCommand("PRAGMA table_info(" + p_strTableName + ");"))
            {
                var table = new DataTable();
                cmd.Connection = p_oConn;
                using (SQLiteDataAdapter adp = new SQLiteDataAdapter(cmd))
                {
                    adp.Fill(table);
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        if (table.Rows[i]["name"].ToString().ToUpper().Trim().Equals(p_strColumnName.ToUpper().Trim()))
                        {
                            bExists = true;
                            break;
                        }
                    }
                 }
            }
            return bExists;
        }

        public string GetConnectionString(string strDatabasePath)
        {
            return "data source = " + strDatabasePath;
        }
        public void DisposedEvent(object sender, EventArgs args)
        {
            _bConnectionDisposed = true;
        }
        public bool DisplayErrors
        {
            get { return _bDisplayErrors; }
            set { _bDisplayErrors = value; }
        }
        public int ErrorCodeTableNotFound
        {
            get { return -100; }
        }
        public int ErrorCodeColumnNotFound
        {
            get { return -200; }
        }
        public int ErrorCodeNoErrors
        {
            get { return 0; }
        }
        public int ErrorCodeTableEmpty
        {
            get { return -2; }
        }

		
		public string MessageBoxTitle
		{
			get {return _strMsgBoxTitle;}
			set {_strMsgBoxTitle=value;}
		}

		


	

		


	}
    public static class Global
    {
        
        public static SQLite.ADO.DataMgr g_oAdoSQLite = new DataMgr();

    }



}
