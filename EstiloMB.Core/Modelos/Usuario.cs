using Chargeback.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;

namespace EstiloMB.Core
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsuarioID { get; set; }

        [MaxLength(50), LogProperty]
        public string Nome { get; set; }

        [MaxLength(50), LogProperty]
        public string Senha { get; set; }

        [MaxLength(20), LogProperty]
        public string Celular { get; set; }
        public DateTime DataNascimento { get; set; }

        [MaxLength(50), LogProperty]
        public string Email { get; set; }

        public string ImageURL { get; set; }
        public string ResetSenhaChave { get; set; }
        public DateTime? ResetSenhaDataLimite { get; set; }
        public bool ResetSenhaLogin { get; set; }
        public DateTime RegistradoEm { get; set; }
        public int? RegistradoPor { get; set; }
        public DateTime? AtualizadoEm { get; set; }
        public int? AtualizadoPor { get; set; }
        public bool IsAdmin { get; set; }
        [LogProperty] public List<UsuarioPerfil> Perfis { get; set; }

        [NotMapped] public string NovaSenha { get; set; }
        [NotMapped] public bool LembrarMe { get; set; }
        [NotMapped] public byte[] ImageData { get; set; }

        public const string PasswordKey = "123abc456def";
        public const int EstiloMbID = 2;

        public static Response<Usuario> Carregar(int userID)
        {
            Response<Usuario> response = new Response<Usuario>();

            try
            {
                using (Database<Usuario> database = new Database<Usuario>())
                {
                    response.Data = database.Set<Usuario>()
                                            .AsNoTracking()
                                            .Include(e => e.Perfis).ThenInclude(e => e.Perfil).ThenInclude(e => e.Acoes)
                                            .FirstOrDefault(e => e.UsuarioID == userID);

                    if (response.Data == null)
                    {
                        response.Code = ResponseCode.NotFound;
                        response.Message = Text.UsuarioNaoEncontrado;
                        return response;
                    }

                    //if (response.Data.Status != StatusRegistro.Ativo)
                    //{
                    //    response.Code = ResponseCode.Denied;
                    //    response.Message = Text.AcessoNegado;
                    //    return response;
                    //}

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
                    Entity = Text.Usuarios,
                    EntityID = userID,
                    LogType = LogTipo.Exception,
                    UserID = userID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
                .Save();
            }

            return response;
        }

        public static Response<List<Usuario>> Listar(Request<Usuario> request)
        {
            Response<List<Usuario>> response = new Response<List<Usuario>>();

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
                if (!user.Data.Perfis.Any(e => e.Perfil.Acoes.Any(e => e.Nome == Text.Aprovador && e.Habilitado == true)))
                {
                    response.Code = ResponseCode.Denied;
                    response.Message = Text.AcessoNegado;
                    return response;
                }

                using (Database<Usuario> database = new Database<Usuario>())
                {
                    IQueryable<Usuario> query = database.Set<Usuario>()
                                                        .AsNoTracking()
                                                        .Include(e => e.Perfis)
                                                        .OrderBy(e => e.Email);

                    if (request.Data?.Perfis?.Count > 0)
                    {
                        List<string> acoes = request.Data.Perfis.SelectMany(e => e.Perfil.Acoes).Select(e => e.Nome).ToList();

                        query = query.Where(e => e.Perfis.Any(p => p.Perfil.Acoes.Any(a => acoes.Contains(a.Nome) && a.Habilitado)));
                    }

                    // - Paginação.
                    response.Total = query.Count();
                    if (request.Page > 0) { query = query.Skip(request.PerPage * (request.Page - 1)); }
                    if (request.PerPage > 0) { query = query.Take(request.PerPage); }

                    // - Resultado.
                    response.Data = query.ToList();
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
                    Entity = Text.Usuarios,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
                .Data(request.Data)
                .Save();
            }

            return response;
        }

        public static Response<Usuario> Salvar(Request<Usuario> request)
        {
            Response<Usuario> response = new Response<Usuario>() { Data = request.Data };

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
                if (!user.Data.Perfis.Any(e => e.Perfil.Acoes.Any(e => e.Nome == Text.Aprovador && e.Habilitado == true)))
                {
                    response.Code = ResponseCode.Denied;
                    response.Message = Text.AcessoNegado;
                    return response;
                }

                using (Database<Usuario> database = new Database<Usuario>())
                {
                    // - Validação.
                    response.Validation = Validation.Validate(request.Data);

                    // - Validação de mesmo email.
                    if (database.Set<Usuario>().Count(e => e.Email == request.Data.Email && e.UsuarioID != request.Data.UsuarioID) > 0)
                    {
                        response.Validation.IsValid = false;
                        response.Validation.Add<Usuario>(Text.UsuarioEmailJaRegistrado, e => e.Email);
                    }

                    // - Erro de validação.
                    if (!response.Validation.IsValid)
                    {
                        response.Code = ResponseCode.Invalid;
                        response.Message = Text.ErroValidacao;
                        return response;
                    }

                    // - Obtendo o Usuário original.
                    Usuario original = response.Data.UsuarioID != 0 ? database.Set<Usuario>()
                                                                              .AsNoTracking()
                                                                              .Include(e => e.Perfis)
                                                                              .FirstOrDefault(e => e.UsuarioID == request.Data.UsuarioID)
                                                                              : null;

                    //request.Data.Status = StatusRegistro.Ativo;
                    request.Data.RegistradoPor = original?.RegistradoPor ?? request.UserID;
                    request.Data.RegistradoEm = original?.RegistradoEm ?? DateTime.Now;
                    request.Data.AtualizadoPor = request.UserID;
                    request.Data.AtualizadoEm = DateTime.Now;

                    if (request.Data.UsuarioID == 0)
                    {
                        request.Data.Senha = AES.Encrypt(request.Data.Senha, PasswordKey);

                        database.Set<Usuario>().Add(request.Data);
                        database.SaveChanges();

                        new Log()
                        {
                            Entity = Text.Usuarios,
                            EntityID = request.Data.UsuarioID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.UsuarioAdicionado
                        }
                        .Data(user.Data.Nome, request.Data.Nome)
                        .Data(null, request.Data)
                        .Save();
                    }
                    else
                    {
                        if (request.Data.Senha != original.Senha)
                        {
                            request.Data.Senha = AES.Encrypt(request.Data.Senha, PasswordKey);
                        }

                        database.Set<UsuarioPerfil>().RemoveRange(original.Perfis.Where(e => !request.Data.Perfis.Select(r => r.PerfilID).Contains(e.PerfilID)));
                        for (int i = 0; i < request.Data.Perfis.Count; i++)
                        {
                            request.Data.Perfis[i].UsuarioPerfilID = original.Perfis.FirstOrDefault(e => e.PerfilID == request.Data.Perfis[i].PerfilID)?.UsuarioPerfilID ?? 0;
                        }

                        database.Set<Usuario>().Update(request.Data);
                        database.SaveChanges();

                        new Log()
                        {
                            Entity = Text.Usuarios,
                            EntityID = request.Data.UsuarioID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.UsuarioAtualizado
                        }
                        .Data(user.Data.Nome, request.Data.Nome)
                        .Data(original, request.Data)
                        .Save();
                    }

                    string directory = Environment.CurrentDirectory + "\\wwwroot\\Usuarios\\" + request.Data.UsuarioID.ToString() + "\\";

                    //currentDirectory = directory + "Perfil\\";
                    if (!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

                    // - Imagem vazia ou nova imagem, apagando a anterior se existir.
                    if ((request.Data.ImageURL == null || request.Data.ImageData != null) && original?.ImageURL != null)
                    {
                        File.Delete(directory + original.ImageURL);
                    }

                    // - Salvando a nova imagem recebida.
                    if (request.Data.ImageData != null && request.Data.ImageData.Length > 0)
                    {
                        File.WriteAllBytes(directory + request.Data.ImageURL, request.Data.ImageData);
                    }
                }

                response.Code = ResponseCode.Sucess;
                response.Message = Text.Sucesso;
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    Entity = Text.Usuarios,
                    EntityID = request.Data.UsuarioID,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
                .Data(request.Data)
                .Save();
            }

            return response;
        }

        public static Response<Usuario> Remover(Request<Usuario> request)
        {
            Response<Usuario> response = new Response<Usuario>() { Data = request.Data };

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
                if (!user.Data.Perfis.Any(e => e.Perfil.Acoes.Any(e => e.Nome == Text.Aprovador && e.Habilitado == true)))
                {
                    response.Code = ResponseCode.Denied;
                    response.Message = Text.AcessoNegado;
                    return response;
                }

                using (Database<Usuario> database = new Database<Usuario>())
                {
                    // - Modelo original.
                    Usuario original = database.Set<Usuario>().FirstOrDefault(e => e.UsuarioID == request.Data.UsuarioID);
                    if (original == null)
                    {
                        response.Code = ResponseCode.BadRequest;
                        response.Message = Text.ErroRequisicao;
                        return response;
                    }

                    //original.Status = StatusRegistro.Removido;
                    request.Data.RegistradoPor = original?.RegistradoPor ?? request.UserID;
                    request.Data.RegistradoEm = original?.RegistradoEm ?? DateTime.Now;
                    request.Data.AtualizadoPor = request.UserID;
                    request.Data.AtualizadoEm = DateTime.Now;

                    database.Set<Usuario>().Update(original);
                    database.SaveChanges();

                    new Log()
                    {
                        Entity = Text.Usuarios,
                        EntityID = request.Data.UsuarioID,
                        LogType = LogTipo.Historico,
                        UserID = request.UserID,
                        Message = Text.UsuarioRemovido
                    }
                    .Data(user.Data.Nome, request.Data.Nome)
                    .Data(original)
                    .Save();
                }

                response.Code = ResponseCode.Sucess;
                response.Message = Text.Sucesso;
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    Entity = Text.Usuarios,
                    EntityID = request.Data.UsuarioID,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
                .Data(request.Data)
                .Save();
            }

            return response;
        }

        public static Response<Usuario> Carregar(string passwordResetKey)
        {
            Response<Usuario> response = new Response<Usuario>() { Data = new Usuario() { ResetSenhaChave = passwordResetKey } };

            try
            {
                using (Database<Usuario> database = new Database<Usuario>())
                {
                    // - Obtendo o Usuário.
                    response.Data = database.Set<Usuario>().AsNoTracking().FirstOrDefault(e => e.ResetSenhaChave == passwordResetKey);
                    if (response.Data == null)
                    {
                        response.Code = ResponseCode.NotFound;
                        response.Message = Text.UsuarioNaoEncontrado;
                        return response;
                    }

                    // - Verificando se ele está ativo.
                    //if (response.Data.Status != StatusRegistro.Ativo)
                    //{
                    //    response.Code = ResponseCode.Denied;
                    //    response.Message = Text.AcessoNegado;
                    //    return response;
                    //}

                    if (DateTime.Now > response.Data.ResetSenhaDataLimite)
                    {
                        response.Code = ResponseCode.BadRequest;
                        response.Message = Text.UsuarioPrazoExpirou;
                    }
                    else
                    {
                        response.Code = ResponseCode.Sucess;
                        response.Message = Text.Sucesso;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    Entity = Text.Usuarios,
                    LogType = LogTipo.Exception,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
                .Save();
            }

            return response;
        }

        public static Response<Usuario> Login(Request<Usuario> request)
        {
            Response<Usuario> response = new Response<Usuario>();

            try
            {
                using (Database<Usuario> database = new Database<Usuario>())
                {
                    Usuario user = null;

                    // - Sem guid, login normal via email/senha.
                    if (request.Data.ResetSenhaChave == null)
                    {
                        // - Obtendo todos os usuários com este email.
                        IList<Usuario> list = database.Set<Usuario>()
                                                      .Include(e => e.Perfis).ThenInclude(e => e.Perfil).ThenInclude(e => e.Acoes)
                                                      .Where(e => e.Email == request.Data.Email)
                                                      .AsNoTracking().ToList();
                        if (list.Count == 0)
                        {
                            response.Validation = new Validation();
                            response.Validation.Add<Usuario>(Text.UsuarioNaoEncontrado, e => e.Email);

                            response.Code = ResponseCode.NotFound;
                            response.Message = Text.UsuarioNaoEncontrado;
                            return response;
                        }

                        // - Procurando por um Usuário com a senha recebida.
                        string encryptedPassword = AES.Encrypt(request.Data.Senha, PasswordKey);
                        user = list.FirstOrDefault(e => e.Senha == encryptedPassword);
                        if (user == null)
                        {
                            response.Validation = new Validation();
                            response.Validation.Add<Usuario>(Text.UsuarioSenhaIncorreta, e => e.Senha);

                            response.Code = ResponseCode.WrongCredentials;
                            response.Message = Text.UsuarioSenhaIncorreta;
                            return response;
                        }

                        if (user.ResetSenhaLogin)
                        {
                            if (request.Data.NovaSenha == null)
                            {
                                response.Validation = new Validation();
                                response.Validation.Add<Usuario>(Text.UsuarioRequerTrocaSenha, e => e.Senha);
                                response.Code = ResponseCode.Expired;
                                response.Message = Text.UsuarioRequerTrocaSenha;
                                return response;
                            }

                            if (request.Data.NovaSenha == user.Senha)
                            {
                                response.Validation = new Validation();
                                response.Validation.Add<Usuario>(Text.SenhaNaoPodeSerIgualAnterior, e => e.NovaSenha);
                                response.Code = ResponseCode.Invalid;
                                response.Message = Text.SenhaNaoPodeSerIgualAnterior;
                                return response;
                            }

                            user.Senha = AES.Encrypt(request.Data.NovaSenha, PasswordKey);
                            user.ResetSenhaLogin = false;
                            user.AtualizadoEm = DateTime.Now;
                            user.AtualizadoPor = user.UsuarioID;

                            database.Update(user);
                            database.SaveChanges();
                        }
                    }
                    else // Com guid, login via guid/senha (trocando a senha)
                    {
                        user = database.Set<Usuario>().AsNoTracking()
                                                      .Include(e => e.Perfis).ThenInclude(e => e.Perfil).ThenInclude(e => e.Acoes)
                                                      .Where(e => e.ResetSenhaChave == request.Data.ResetSenhaChave)
                                                      .FirstOrDefault();
                        if (user == null)
                        {
                            response.Validation = new Validation();
                            response.Validation.Add<Usuario>(Text.UsuarioNaoEncontrado, e => e.ResetSenhaChave);

                            response.Code = ResponseCode.NotFound;
                            response.Message = Text.UsuarioNaoEncontrado;
                            return response;
                        }

                        if (DateTime.Now > user.ResetSenhaDataLimite)
                        {
                            response.Validation = new Validation();
                            response.Validation.Add<Usuario>(Text.UsuarioPrazoExpirou, e => e.ResetSenhaDataLimite);

                            response.Code = ResponseCode.BadRequest;
                            response.Message = Text.UsuarioPrazoExpirou;
                            return response;
                        }

                        user.ResetSenhaChave = null;
                        user.ResetSenhaDataLimite = null;
                        user.Senha = AES.Encrypt(request.Data.NovaSenha, PasswordKey);
                        user.AtualizadoEm = DateTime.Now;
                        user.AtualizadoPor = user.UsuarioID;

                        database.Update(user);
                        database.SaveChanges();
                    }

                    //Atividades.Adicionar(usuario);

                    response.Data = user;
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
                    Entity = Text.Usuarios,
                    EntityID = request.Data.UsuarioID,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
                .Data(request.Data)
                .Save();
            }

            return response;
        }

        public static void Logout(string enderecoIP)
        {
            //Atividade.Remover(enderecoIP);
        }

        public static Response<Usuario> Esqueci(string email, string redirectURL)
        {
            Response<Usuario> response = new Response<Usuario>() { Data = new Usuario() { Email = email } };

            try
            {
                using (Database<Usuario> database = new Database<Usuario>())
                {
                    // - Obtendo o Usuário.
                    Usuario user = database.Set<Usuario>().AsNoTracking().FirstOrDefault(e => e.Email == email);
                    if (user == null)
                    {
                        response.Validation = new Validation();
                        response.Validation.Add<Usuario>(Text.UsuarioNaoEncontrado, e => e.Email);

                        response.Code = ResponseCode.NotFound;
                        response.Message = Text.UsuarioNaoEncontrado;
                        return response;
                    }

                    user.ResetSenhaChave = Guid.NewGuid().ToString();
                    user.ResetSenhaDataLimite = DateTime.Now.AddDays(1);

                    database.Update(user);
                    database.SaveChanges();

                    string html = null;
                    Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Agro.Core.Resources.ResetPassword.html");
                    using (StreamReader reader = new StreamReader(resource))
                    {
                        html = reader.ReadToEnd();
                    }

                    html = html.Replace("@nome", user.Nome);
                    html = html.Replace("@url", redirectURL + user.ResetSenhaChave);
                    //html = html.Replace("@ano", DateTime.Now.Year.ToString());

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress("recuperacao-senha@kstack.com.br");
                        mail.To.Add(user.Email);
                        mail.Body = html;
                        mail.IsBodyHtml = true;
                        mail.Subject = "Recuperação de Senha";

                        using (SmtpClient client = new SmtpClient())
                        {
                            client.Host = "smtp.kskmail.com.br";
                            client.Port = 587;
                            client.EnableSsl = false;
                            client.Credentials = new NetworkCredential("rsenha@kskmail.com.br", "senha@123");

                            try
                            {
                                client.Send(mail);
                            }
                            catch (Exception ex)
                            {
                                response.Code = ResponseCode.BadRequest;
                                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                                new Log()
                                {
                                    Entity = Text.Usuarios,
                                    EntityID = user.UsuarioID,
                                    LogType = LogTipo.Exception,
                                    UserID = user.UsuarioID,
                                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                                }
                                .Save();
                                return response;
                            }
                        }
                    }

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
                    Entity = Text.Usuarios,
                    LogType = LogTipo.Exception,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
                .Save();
            }

            return response;
        }

        public void Tick(string enderecoIP)
        {
            //Atividade.Tick(UserID, enderecoIP);
        }
    }
}