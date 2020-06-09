/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CRL.Core;
using CRL.LambdaQuery;
namespace CRL.DBExtend.RelationDB
{
    public sealed partial class DBExtend
    {
        #region sql to sp

        static Dictionary<string, Dictionary<string, int>> spCahe = new Dictionary<string, Dictionary<string, int>>();
        /// <summary>
        /// 将SQL语句编译成储存过程
        /// </summary>
        /// <param name="template">模版</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parames">模版替换参数</param>
        /// <returns></returns>
        string CompileSqlToSp(string template, string sql, Dictionary<string, string> parames = null)
        {
            if (!_DBAdapter.CanCompileSP)
            {
                throw new CRLException("当前数据库不支持动态编译");
            }
            sql = _DBAdapter.SqlFormat(sql);
            var dbName = dbContext.DBHelper.DatabaseName;
            lock (lockObj)
            {
                if (!spCahe.ContainsKey(dbName))//初始已编译过的存储过程
                {
                    var db = GetBackgroundDBExtend();
                    //BackupParams();
                    var dic = db.ExecDictionary<string, int>(_DBAdapter.GetAllSPSql(dbContext.DBHelper.DatabaseName));
                    if (!spCahe.ContainsKey(dbName))
                    {
                        spCahe.Add(dbName, dic);
                    }
                    //RecoveryParams();
                }
            }
            string fields = "";
            if (parames != null)
            {
                if (parames.ContainsKey("fields"))
                {
                    fields = parames["fields"];
                }
                if (parames.ContainsKey("sort"))
                {
                    fields += "_" + parames["sort"];
                }
                if (parames.ContainsKey("rowOver"))
                {
                    fields += "_" + parames["rowOver"];
                }
            }
            string spPrex = $"ZautoSp_";
            string sp;
            if(SettingConfig.FieldParameName)
            {
                spPrex += "H_";
            }
            sp = StringHelper.EncryptMD5(fields + "_" + sql.Trim());
            sp = spPrex + sp.Substring(8, 16);

            var dbCahce = spCahe[dbName];
            if (!dbCahce.ContainsKey(sp))
            {
                //sql = __DbHelper.FormatWithNolock(sql);
                var db = GetBackgroundDBExtend();
                FillParame(dbContext.DBHelper);
                string spScript = Base.SqlToProcedure(template, dbContext, sql, sp, parames);
                try
                {
                    //BackupParams();
                    db.dbContext.DBHelper.Execute(spScript);
                    //RecoveryParams();
                    lock (lockObj)
                    {
                        if (!dbCahce.ContainsKey(sp))
                        {
                            dbCahce.Add(sp, 0);
                        }
                    }
                    string log = string.Format("创建存储过程:{0}\r\n{1}", sp, spScript);
                    EventLog.Log(log, "sqlToSp", false);
                }
                catch (Exception ero)
                {
                    //RecoveryParams();
                    throw new CRLException("动态创建存储过程时发生错误:" + ero.Message);
                }
            }
            return sp;
        }
        #endregion
    }
}
