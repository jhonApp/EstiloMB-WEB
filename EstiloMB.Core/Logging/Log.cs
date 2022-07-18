using EstiloMB.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Chargeback.Core
{
    [Table("Log")]
    public class Log
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogID { get; set; }

        public LogTipo LogType { get; set; }
        public string Entity { get; set; }
        public int? EntityID { get; set; }

        public string File { get; set; }
        public string Function { get; set; }
        public int Line { get; set; }

        public int? UserID { get; set; }
        [MaxLength(4000)] public string Message { get; set; }
        public DateTime AddedOn { get; set; }

        public List<Parameter> MessageParameters { get; set; }
        public List<LogData> EntityData { get; set; }
        public Usuario User { get; set; }
        [NotMapped] public ChangeLog ChangeLog { get; set; }

        [NotMapped] public DateTime? From { get; set; }
        [NotMapped] public DateTime? Until { get; set; }

        public Log() { }

        public Log Parameters(params string[] parametros)
        {
            if (MessageParameters == null)
            {
                MessageParameters = new List<Parameter>();
            }

            for (int i = 0; i < parametros.Length; i++)
            {
                MessageParameters.Add(new Parameter()
                {
                    Value = parametros[i]
                });
            }

            return this;
        }

        public Log Data(object original, object updated = null)
        {
            if (EntityData == null)
            {
                EntityData = new List<LogData>();
            }

            if (original != null)
            {
                EntityData.Add(new LogData()
                {
                    DataType = DataType.Original,
                    Type = original.GetType().FullName,
                    Value = JsonConvert.SerializeObject(original)
                });
            }

            if (updated != null)
            {
                EntityData.Add(new LogData()
                {
                    DataType = DataType.Updated,
                    Type = updated.GetType().FullName,
                    Value = JsonConvert.SerializeObject(updated)
                });
            }

            return this;
        }

        public bool Save([CallerFilePath] string file = null, [CallerMemberName] string function = null, [CallerLineNumber] int line = 0)
        {
            File = file;
            Function = function;
            Line = line;
            AddedOn = DateTime.Now;

            try
            {
                using (Database<Log> database = new Database<Log>())
                {
                    database.Set<Log>().Add(this);
                    database.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                SaveTXT(ex.Message, this);
                return false;
            }

            return true;
        }

        public static void SaveTXT(string message, Log log)
        {
            try
            {
                Directory.CreateDirectory("Logs");
                System.IO.File.AppendAllText("Logs/Log-" + DateTime.Now.ToString("yyyy-MM-dd"), JsonConvert.SerializeObject(log));
            }
            catch
            {
                // - Se deu erro até aqui, vish...
            }
        }

        public static Response<List<Log>> Listar(Request<Log> request)
        {
            Response<List<Log>> response = new Response<List<Log>>();

            try
            {
                // - Usuário que chamou esta ação.
                Response<Usuario> user = Usuario.Carregar(request.UserID);
                if (user.Code != ResponseCode.Sucess)
                {
                    response.Code = user.Code;
                    response.Message = user.Message;
                    return response;
                }

                // - Verificando permissão.
                if (!user.Data.Kstack && !user.Data.Admin && !user.Data.Perfis.Any(e => e.Perfil.Acoes.Any(e => e.Nome == Text.Log)))
                {
                    response.Code = ResponseCode.Denied;
                    response.Message = Text.AcessoNegado;
                    return response;
                }

                using (Database<Log> database = new Database<Log>())
                {
                    IQueryable<Log> query = database.Set<Log>()
                                                    .Include(e => e.User)
                                                    .Include(e => e.MessageParameters)
                                                    .Include(e => e.EntityData)
                                                    .AsNoTracking()
                                                    .OrderByDescending(e => e.AddedOn);

                    if (request.Data?.From != null)
                    {
                        query = query.Where(e => e.AddedOn.Date >= request.Data.From.Value.Date);
                    }

                    if (request.Data?.Until != null)
                    {
                        query = query.Where(e => e.AddedOn.Date <= request.Data.Until.Value.Date);
                    }

                    // - Paginação.
                    response.Total = query.Count();
                    if (request.Page > 0) { query = query.Skip(request.PerPage * (request.Page - 1)); }
                    if (request.PerPage > 0) { query = query.Take(request.PerPage); }

                    List<Log> logs = query.ToList();
                    for (int i = 0; i < logs.Count; i++)
                    {
                        if (logs[i].EntityData.Count == 0) { continue; }

                        Type tipo = Type.GetType(logs[i].EntityData[0].Type);

                        LogData originalData = logs[i].EntityData.FirstOrDefault(e => e.DataType == DataType.Original);
                        LogData updatedData = logs[i].EntityData.FirstOrDefault(e => e.DataType == DataType.Updated);

                        object original = originalData != null ? JsonConvert.DeserializeObject(originalData.Value, tipo) : null;
                        object alterado = updatedData != null ? JsonConvert.DeserializeObject(updatedData.Value, tipo) : null;

                        logs[i].ChangeLog = GetChangeLog(original, alterado);
                    }

                    response.Data = logs;
                    response.Code = ResponseCode.Sucess;
                    response.Message = Text.Sucesso;
                }
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    Entity = Text.Log,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
                .Data(request.Data)
                .Save();
            }

            return response;
        }

        private static ChangeLog GetChangeLog(object original, object alterado)
        {
            Type tipo = original?.GetType() ?? alterado?.GetType();
            PropertyInfo[] properties = tipo.GetProperties();
            DisplayNameAttribute attribute = Attribute.GetCustomAttribute(tipo, typeof(DisplayNameAttribute)) as DisplayNameAttribute;

            List<ChangeLog> changes = new List<ChangeLog>();
            ChangeLog changeLog = new ChangeLog();
            changeLog.Name = attribute?.DisplayName ?? tipo.Name;
            changeLog.Value = changes;
            changeLog.Operation = original != null && alterado != null ? ChangeLogOperation.Update : original == null && alterado != null ? ChangeLogOperation.Insert : original != null && alterado == null ? ChangeLogOperation.Delete : ChangeLogOperation.NotAvailable;

            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].GetCustomAttribute<LogProperty>() == null) { continue; }

                ChangeLog property = new ChangeLog();
                changes.Add(property);

                property.Name = properties[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? properties[i].Name;

                if (!properties[i].PropertyType.IsArray && !properties[i].PropertyType.Equals(typeof(string)) && !properties[i].PropertyType.Equals(typeof(object)) && properties[i].PropertyType.GetInterface(nameof(IEnumerable)) != null)
                {
                    List<ChangeLog> changeList = new List<ChangeLog>();
                    Type propertyType = properties[i].PropertyType.IsGenericType ? properties[i].PropertyType.GetGenericArguments()[0] : properties[i].PropertyType;
                    PropertyInfo propertyKey = propertyType.GetProperties().FirstOrDefault(e => e.GetCustomAttribute<KeyAttribute>() != null);

                    IList originalList = original != null ? properties[i].GetValue(original) as IList : null;
                    IList updatedList = alterado != null ? properties[i].GetValue(alterado) as IList : null;

                    if (originalList != null)
                    {
                        foreach (object item in originalList)
                        {
                            string id = propertyKey.GetValue(item).ToString();
                            object counterpart = null;
                            if (updatedList != null)
                            {
                                foreach (object updatedItem in updatedList)
                                {
                                    if (propertyKey.GetValue(updatedItem).ToString() == id)
                                    {
                                        counterpart = updatedItem;
                                        updatedList.Remove(updatedItem);
                                        break;
                                    }
                                }
                            }

                            changeList.Add(GetChangeLog(item, counterpart));
                        }
                    }

                    if (updatedList != null)
                    {
                        foreach (object item in updatedList)
                        {
                            changeList.Add(GetChangeLog(null, item));
                        }
                    }

                    property.Value = changeList;
                    property.Type = ChangeLogType.List;
                    property.Operation = ChangeLogOperation.NotAvailable;
                    continue;
                }
                else if (properties[i].PropertyType.IsClass && !properties[i].PropertyType.IsArray && !properties[i].PropertyType.Equals(typeof(string)) && !properties[i].PropertyType.Equals(typeof(object)))
                {
                    object classeOriginal = original != null ? properties[i].GetValue(original) : null;
                    object classeAlterada = alterado != null ? properties[i].GetValue(alterado) : null;

                    if (classeOriginal != null || classeAlterada != null)
                    {
                        property.Value = GetChangeLog(classeOriginal, classeAlterada);
                    }

                    property.Type = ChangeLogType.Object;
                    property.Operation = ChangeLogOperation.NotAvailable;
                    continue;
                }

                property.Value = original != null ? properties[i].GetValue(original) : null;
                property.NewValue = alterado != null ? properties[i].GetValue(alterado) : null;
                property.Type = properties[i].PropertyType == typeof(DateTime) ? ChangeLogType.Date : ChangeLogType.String;
                property.Operation = property.Value != null && property.NewValue == null ? ChangeLogOperation.Delete :
                                     property.Value == null && property.NewValue != null ? ChangeLogOperation.Insert :
                                     property.Value == null && property.NewValue == null ? ChangeLogOperation.NotAvailable :
                                     property.Value.ToString() == property.NewValue.ToString() ? ChangeLogOperation.Unmodified :
                                     ChangeLogOperation.Update;
            }

            return changeLog;
        }
    }
}