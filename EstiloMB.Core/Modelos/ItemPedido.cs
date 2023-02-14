﻿using Chargeback.Core;
using Microsoft.EntityFrameworkCore;
using Sistema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;

namespace EstiloMB.Core
{
    [Table("Pedido.Item")]
    public class ItemPedido
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Cor { get; set; }
        public string Tamanho { get; set; }
        public string ImageURL { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorTotal { get; set; }
        public int PedidoID { get; set; }
        public int ProdutoID { get; set; }

        [ForeignKey("PedidoID")]
        public Pedido Pedido { get; set; }

        [ForeignKey("ProdutoID")]
        public Produto Produto { get; set; }

        public static Response<List<ItemPedido>> Listar(Request<ItemPedido> request)
        {
            Response<List<ItemPedido>> response = new Response<List<ItemPedido>>();

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

                using (Database<ItemPedido> database = new Database<ItemPedido>())
                {
                    IQueryable<ItemPedido> query = database.Set<ItemPedido>()
                                                       .AsNoTracking()
                                                       .OrderByDescending(e => e.ID);

                    response.Total = query.Count();
                    if (request.Page > 0) { query = query.Skip(request.PerPage * (request.Page - 1)); }
                    if (request.PerPage > 0) { query = query.Take(request.PerPage); }

                    response.Data = query.ToList();
                    response.Code = ResponseCode.Sucess;
                }
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
               .Data(request.Data)
               .Save();
            }

            return response;
        }

        //public static ItemPedido GetItemPedidoByUsuario(int usuarioID)
        //{
        //    try
        //    {
        //        using Database<ItemPedido> db = new Database<ItemPedido>();
        //        return db.Set<ItemPedido>()
        //            .Include(e => e.ProdutoCategorias)
        //            .Include(e => e.ProdutoTamanhos).ThenInclude(e => e.Tamanho)
        //            .Include(e => e.ProdutoImagens).ThenInclude(e => e.Cor)
        //            .Include(e => e.ProdutoImagens)
        //            .Where(p => p.ID == produtoID)
        //            .FirstOrDefault();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public static Response<ItemPedido> Salvar(Request<ItemPedido> request)
        {
            Response<ItemPedido> response = new Response<ItemPedido>() { Data = request.Data };

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

                using (Database<ItemPedido> database = new Database<ItemPedido>())
                {

                    // - Validação.
                    response.Validation = Validation.Validate(request.Data);
                    // - Erro de validação.
                    if (!response.Validation.IsValid)
                    {
                        response.Code = ResponseCode.Invalid;
                        response.Message = JHIException.ErroValidacao;
                        return response;
                    }

                    // - Modelo original.
                    ItemPedido original = database.Set<ItemPedido>()
                                                .AsNoTracking()
                                                .FirstOrDefault(e => e.ID == request.Data.ID);


                    if (request.Data.ID == 0)
                    {
                        database.Set<ItemPedido>().Add(request.Data);
                        database.SaveChanges();

                        new Log()
                        {
                            Entity = Text.ItemPedido,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.ItemCriado
                        }
                       .Parameters(usuario.Data.Nome)
                       .Data(null, request.Data)
                       .Save();
                    }
                    else
                    {
                        database.Set<ItemPedido>().Update(request.Data);
                        database.SaveChanges();

                        new Log()
                        {
                            Entity = Text.ItemPedido,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.ItemCriado
                        }
                       .Parameters(usuario.Data.Nome)
                       .Data(null, request.Data)
                       .Save();
                    }
                }

                response.Code = ResponseCode.Sucess;
                response.Message = JHIException.Sucesso;
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    Entity = Text.ItemPedido,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
               .Data(request.Data)
               .Save();
            }

            return response;
        }

        public static ItemPedido CreateItemPedido(string tamanho, Pedido pedido, int produtoID, int corID)
        {
            ItemPedido itemPedido = new ItemPedido();

            Produto produto = Produto.GetByID(produtoID);
            try
            {
                if(produto != null)
                {
                    using Database<ItemPedido> database = new Database<ItemPedido>();

                    if(pedido != null)
                    {
                        ProdutoImagem produtoImagem = produto.ProdutoImagens.Where(e => e.CorID == corID).FirstOrDefault();

                        itemPedido.PedidoID = pedido.ID;
                        itemPedido.ProdutoID = produto.ID;
                        itemPedido.Cor = produtoImagem.Cor.Nome;
                        itemPedido.ImageURL = produtoImagem.ImageURL;
                        itemPedido.Tamanho = tamanho;
                        itemPedido.Quantidade = 1;
                        itemPedido.ValorTotal = produto.Valor;

                        database.Set<ItemPedido>().Add(itemPedido);
                        database.SaveChanges();

                        itemPedido.Produto = produto;
                        itemPedido.Pedido = pedido;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return itemPedido;
        }

        public static List<ItemPedido> GetByPedido(int pedidoID)
        {
            try
            {
                using Database<ItemPedido> db = new Database<ItemPedido>();
                decimal sum = 0;

                List<ItemPedido> itemPedidos = db.Set<ItemPedido>()
                    .Include(p => p.Pedido)
                    .Include(p => p.Produto)
                    .Include(p => p.Produto).ThenInclude(p => p.ProdutoImagens)
                    .Where(p => p.PedidoID == pedidoID)
                    .ToList();

                foreach (var item in itemPedidos)
                {
                    //arrumar o calculo do itempedido (valorUnitario x quantidade)
                    sum = item.ValorTotal + sum;
                    item.Pedido.ValorTotal = sum;
                }

                return itemPedidos;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ItemPedido Get(int produtoID, string tamanho, int pedidoID)
        {
            try
            {
                using Database<ItemPedido> db = new Database<ItemPedido>();
                return db.Set<ItemPedido>()
                    .Include(p => p.Produto)
                    .Where(p => p.ProdutoID == produtoID && p.Tamanho == tamanho && p.PedidoID == pedidoID)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<ItemPedido> GetAll(int pedidoID)
        {
            try
            {
                using Database<ItemPedido> db = new Database<ItemPedido>();
                return db.Set<ItemPedido>()
                    .Include(p => p.Produto)
                    .Where(p => p.PedidoID == pedidoID)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Produto SaveByProdutoID(int produtoID)
        {
            try
            {
                using Database<Produto> db = new Database<Produto>();
                return db.Set<Produto>()
                    .Include(e => e.ProdutoCategorias)
                    .Include(e => e.ProdutoTamanhos).ThenInclude(e => e.Tamanho)
                    .Include(e => e.ProdutoImagens).ThenInclude(e => e.Cor)
                    .Include(e => e.ProdutoImagens)
                    .Where(p => p.ID == produtoID)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ItemPedido Update(int pedidoID, int produtoID, string tamanho, string nomeCor)
        {
            ItemPedido itemPedido = new ItemPedido();
            Response<ItemPedido> response = new Response<ItemPedido>();
            int quantidade = 0;
            try
            {
                using Database<ItemPedido> db = new Database<ItemPedido>();
                itemPedido = db.Set<ItemPedido>()
                    .Include(p => p.Produto)
                    .Include(p => p.Pedido)
                    .Where(p => p.PedidoID == pedidoID && p.ProdutoID == produtoID && p.Tamanho == tamanho && p.Cor == nomeCor)
                    .FirstOrDefault();
                quantidade = itemPedido.Quantidade+1;
                itemPedido.Quantidade = quantidade;
                itemPedido.ValorTotal = itemPedido.Produto.Valor * quantidade;
                db.Update(itemPedido);
                db.SaveChanges();
            }
            catch (Exception) { throw; }

            return itemPedido;
        }

        public static List<ItemPedido> GerarItemPedido(Pedido pedido, int produtoID, string tamanho, int corID)
        {
            ItemPedido itemPedido = new ItemPedido();
            List<ItemPedido> itemPedidos = GetAll(pedido.ID);
            List<ItemPedido> listaPedidos = new List<ItemPedido>();
            bool registroAtualizado = false;
            ProdutoImagem produtoImagem = ProdutoImagem.GetByCorID(corID);

            for (int i = 0; i < itemPedidos.Count(); i++)
            {
                if (itemPedidos[i].Tamanho == tamanho && itemPedidos[i].Cor == produtoImagem.Cor.Nome && itemPedidos[i].ID != 0)
                {
                    itemPedidos[i] = Update(pedido.ID, produtoID, tamanho, produtoImagem.Cor.Nome);
                    registroAtualizado = true;
                }

                itemPedido = itemPedidos[i];
                listaPedidos.Add(itemPedido);
            }

            if(itemPedidos.Count == 0 || !registroAtualizado)
            {
                listaPedidos.Add(CreateItemPedido(tamanho, pedido, produtoID, corID));
            }
            

            return listaPedidos;
        }

        public static void Remove(int ID)
        {
            try
            {
                using Database<ItemPedido> db = new Database<ItemPedido>();
                ItemPedido itemPedido =  db.Set<ItemPedido>()
                    .Where(p => p.ID == ID)
                    .FirstOrDefault();

                db.Set<ItemPedido>().Remove(itemPedido);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Response<ItemPedido> Remover(Request<ItemPedido> request)
        {
            Response<ItemPedido> response = new Response<ItemPedido>() { Data = request.Data };

            try
            {
                // - Usuário que chamou esta ação.
                //Response<Usuario> usuario = Usuario.Carregar(request.UserID);
                //if (usuario.Code != ResponseCode.Sucess)
                //{
                //    response.Code = usuario.Code;
                //    response.Message = usuario.Message;
                //    return response;
                //}

                using (Database<ItemPedido> database = new Database<ItemPedido>())
                {
                    // - Modelo original.
                    ItemPedido original = response.Data.ID != 0 ? database.Set<ItemPedido>()
                                                                            .AsNoTracking()
                                                                            .FirstOrDefault(e => e.ID == request.Data.ID)
                                                                            : null;
                    if (original == null)
                    {
                        response.Code = ResponseCode.BadRequest;
                        response.Message = JHIException.ErroRequisicao;
                        return response;
                    }

                    database.Set<ItemPedido>().Remove(request.Data);
                    database.SaveChanges();

                    new Log()
                    {
                        Entity = Text.ItemPedido,
                        EntityID = request.Data.ID,
                        LogType = LogTipo.Historico,
                        UserID = request.UserID,
                        Message = Text.ItemPedido
                    }
                       .Parameters()
                       .Data(null, request.Data)
                       .Save();
                }

                response.Code = ResponseCode.Sucess;
                response.Message = JHIException.Sucesso;
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    Entity = Text.ItemPedido,
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