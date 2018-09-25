using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Repository.EF.Core
{
    public class SqlHelper
    {
        /// <summary>
        /// 参数拼装
        /// </summary>
        /// <param name="parameterName">参数名称</param>
        /// <param name="parameterType">参数类型</param>
        /// <param name="parameterValue">参数值</param>
        /// <returns>拼装后的参数对象</returns>
        public static SqlParameter MakeParam(string parameterName, SqlDbType parameterType, object parameterValue)
        {
            SqlParameter p = new SqlParameter();
            p.ParameterName = parameterName;
            p.SqlDbType = parameterType;
            if (parameterValue == null)
            {
                p.Value = DBNull.Value;
            }
            else
            {
                if (parameterType == SqlDbType.Structured)
                {
                    Type type = parameterValue.GetType();
                    if (type.IsGenericType)//判断是否是泛型
                    {
                        string t = type.GetGenericArguments()[0].Name; //泛型的类型
                        switch (t)
                        {
                            case "Guid":
                                p.TypeName = "GuidCollectionTVP";
                                p.Value = InitialCollectionTVP(parameterValue as List<Guid>);
                                break;
                            case "Int32":
                                p.TypeName = "IntCollectionTVP";
                                p.Value = InitialCollectionTVP(parameterValue as List<int>);
                                break;
                            case "String":
                                p.TypeName = "StringCollectionTVP";
                                p.Value = InitialCollectionTVP(parameterValue as List<string>);
                                break;
                            default:
                                p.Value = parameterValue;
                                break;
                        }
                    }
                    else
                    {
                        p.Value = parameterValue;
                    }

                }
                else
                {
                    p.Value = parameterValue;
                }
            }

            return p;
        }

        /*
        /// <summary>
        /// 参数拼装
        /// </summary>
        /// <param name="parameterName">参数名称</param>
        /// <param name="parameterType">参数类型</param>
        /// <param name="parameterValue">参数值</param>
        /// <returns>拼装后的参数对象</returns>
        public static SqlParameter MakeParam(string parameterName, DbType parameterType, object parameterValue)
        {
            SqlParameter p = new SqlParameter();
            p.ParameterName = parameterName;
            p.DbType = parameterType;
            if (parameterValue == null)
            {
                p.Value = DBNull.Value;
            }
            else
            {
                p.Value = parameterValue;
            }

            return p;
        }

        */
        /// <summary>
        /// 初始化CollectionTVP
        /// </summary>
        /// <param name="list">数据列表</param>
        /// <returns>DataTable</returns>
        public static DataTable InitialCollectionTVP<T>(List<T> list)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Item", typeof(T));
            if (list == null | list.Count == 0)
            {
                return dt;
            }

            foreach (T item in list)
            {
                dt.Rows.Add(item);
            }

            return dt;
        }
        /// <summary>
        /// 过滤sql注入2015/7/25 xingyongkang
        /// </summary>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        public static string CheckKeyWord(string keyWord)
        {
            //过滤关键字
            string StrKeyWord = @"select|insert|delete|from|count\(|drop table|update|truncate|asc\(|mid\(|char\(|xp_cmdshell|exec master|netlocalgroup administrators|:|net user|""|or|and";
            if (Regex.IsMatch(keyWord, StrKeyWord, RegexOptions.IgnoreCase))
            {
                return "";
            }
            return keyWord;
        }
    }
}
