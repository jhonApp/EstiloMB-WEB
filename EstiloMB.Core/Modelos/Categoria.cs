﻿using Chargeback.Core;
using Microsoft.EntityFrameworkCore;
using Sistema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace EstiloMB.Core
{
    [Table("Categoria")]
    public class Categoria
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, MaxLength(50), LogProperty]
        public string Nome { get; set; }

        public static Response<List<Categoria>> Listar(Request<Categoria> request)
        {
            Response<List<Categoria>> response = new Response<List<Categoria>>();

            try
            {
                // - Usuário que chamou esta ação.
                Response<Usuario> usuario = Usuario.Carregar(request.UserID);
                if (usuario.Code != ResponseCode.Sucess)
                {
                    response.Code = usuario.Code;
                    response.Message = usuario.Message;
                    return response;
                }

                // - Verificando permissão.
                //if (!usuario.Data.Perfis.Any(e => e.Perfil.Acoes.Any(e => e.Nome == Text.Componentes && e.Habilitado == true)))
                //{
                //    response.Code = ResponseCode.Denied;
                //    response.Message = Text.AcessoNegado;
                //    return response;
                //}

                using (Database<Categoria> database = new Database<Categoria>())
                {
                    IQueryable<Categoria> query = database.Set<Categoria>()
                                                       .AsNoTracking()
                                                       .OrderBy(e => e.Nome);

                    response.Total = query.Count();
                    if (request.Page > 0) { query = query.Skip(request.PerPage * (request.Page - 1)); }
                    if (request.PerPage > 0) { query = query.Take(request.PerPage); }

                    response.Data = query.ToList();
                    response.Code = ResponseCode.Sucess;
                    response.Message = JHIException.Sucesso;
                }
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    Entity = Text.Categoria,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
                .Data(request.Data)
                .Save();
            }

            return response;
        }
    }
}
