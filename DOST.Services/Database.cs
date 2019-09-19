using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data.SqlClient;
using System.Xml.Linq;
using System.Diagnostics;

namespace DOST.Services {
    static class Database {
        private static SqlConnection sql;

        public static void InitializeDatabase() {
            var xmlElements = Engine.GetConfigFileElements();
            sql = new SqlConnection(
                @"Server=" + xmlElements["Database"]["Server"] +
                ";Database=" + xmlElements["Database"]["DatabaseName"] +
                ";Trusted_Connection=False;User ID=" + xmlElements["Database"]["DatabaseUser"] +
                ";Password=" + xmlElements["Database"]["DatabasePassword"]
            );
            try {
                sql.Open();
            } catch (SqlException sqlException) {
                Console.WriteLine("Cannot connect to database.\nError: " + sqlException.Message);
                throw;
            } finally {
                if (sql.State == System.Data.ConnectionState.Open) {
                    sql.Close();
                }
            }
        }

        public static void ExecuteQuery(string query) {
            sql.Open();
            SqlDataAdapter command = new SqlDataAdapter {
                InsertCommand = new SqlCommand(query) {
                    Connection = sql
                }
            };
            command.InsertCommand.ExecuteNonQuery();
            sql.Close();
        }

        public static bool ExecuteUpdate(string query, Dictionary<string, object> queryArguments, Action<Exception> onFail = null) {
            try {
                sql.Open();
                SqlDataAdapter command = new SqlDataAdapter {
                    InsertCommand = new SqlCommand(query, sql)
                };
                if (queryArguments != null) {
                    foreach (var queryArgument in queryArguments) {
                        command.InsertCommand.Parameters.AddWithValue(queryArgument.Key, queryArgument.Value);
                    }
                }
                command.InsertCommand.ExecuteNonQuery();
                sql.Close();
                return true;
            } catch (SqlException sqlException) {
                Console.WriteLine("EU SQLException -> " + sqlException.Message);
                if (onFail != null) {
                    onFail.Invoke(sqlException);
                }
            } catch (Exception e) {
                Console.WriteLine("EU Exception -> " + e.Message.ToString());
                if (onFail != null) {
                    onFail.Invoke(e);
                }
            } finally {
                if (sql.State == System.Data.ConnectionState.Open) {
                    sql.Close();
                }
            }
            return false;
        }

        public static bool ExecuteUpdate(string query, Dictionary<string, object> queryArguments, out int lastInsertedId, Action<Exception> onFail = null) {
            try {
                sql.Open();
                SqlDataAdapter command = new SqlDataAdapter {
                    InsertCommand = new SqlCommand(query, sql)
                };
                if (queryArguments != null) {
                    foreach (var queryArgument in queryArguments) {
                        command.InsertCommand.Parameters.AddWithValue(queryArgument.Key, queryArgument.Value);
                    }
                }
                lastInsertedId = (int) command.InsertCommand.ExecuteScalar();
                sql.Close();
                return true;
            } catch (SqlException sqlException) {
                Console.WriteLine("EU SQLException -> " + sqlException.Message);
                if (onFail != null) {
                    onFail.Invoke(sqlException);
                }
            } catch (Exception e) {
                Console.WriteLine("EU Exception -> " + e.Message.ToString());
                if (onFail != null) {
                    onFail.Invoke(e);
                }
            } finally {
                if (sql.State == System.Data.ConnectionState.Open) {
                    sql.Close();
                }
            }
            lastInsertedId = 0;
            return false;
        }

        public static void ExecuteStoreQuery(
            string query, Dictionary<string, object> queryArguments,
            Action<List<Dictionary<string, object>>> action, Action zeroResults = null
        ) {
            try {
                sql.Open();
                List<DatabaseStruct.SSQLRow> result = new List<DatabaseStruct.SSQLRow>();
                SqlCommand command = new SqlCommand(query, sql);
                if (queryArguments != null) {
                    foreach (var queryArgument in queryArguments) {
                        command.Parameters.AddWithValue(queryArgument.Key, queryArgument.Value);
                    }
                }
                using (SqlDataReader reader = command.ExecuteReader()) {
                    var dataReader = reader.Read();
                    while (dataReader) {
                        DatabaseStruct.SSQLRow row = new DatabaseStruct.SSQLRow(new Dictionary<string, object>());
                        for (int index = 0; index < reader.FieldCount; index++) {
                            try {
                                row.Columns.Add(reader.GetName(index), reader.GetValue(index));
                            } catch (IndexOutOfRangeException) {
                                Console.WriteLine("ESQ Catch Exception");
                                break;
                            }
                        }
                        result.Add(row);
                        dataReader = reader.Read();
                    }
                }
                sql.Close();
                if (result.Count > 0) {
                    if (action != null) {
                        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                        foreach (var row in result) {
                            var rowData = new Dictionary<string, object>();
                            foreach (var column in row.Columns) {
                                rowData.Add(column.Key, column.Value);
                            }
                            data.Add(rowData);
                        }
                        action.Invoke(data);
                    }
                } else {
                    if (zeroResults != null) {
                        zeroResults.Invoke();
                    }
                }
            } catch (SqlException sqlException) {
                Console.WriteLine("ESQ SQLException -> " + sqlException.Message);
            } catch (Exception e) {
                Console.WriteLine("ESQ Exception -> " + e.Message.ToString());
            } finally {
                if (sql.State == System.Data.ConnectionState.Open) {
                    sql.Close();
                }
            }
        }
    }
}
