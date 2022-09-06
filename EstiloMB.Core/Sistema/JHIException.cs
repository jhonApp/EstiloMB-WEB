using System;
using System.Collections.Generic;
using System.Text;

namespace Sistema
{
    public class JHIException
    {
        internal const string ChaveSenhaUsuario = "UnsecurePassword";
        internal const string Sucesso = "Sucesso.";
        internal const string AcessoNegado = "Você não tem permissão para esta operação.";
        internal const string Login = "Login realizado com sucesso.";
        internal const string ErroRequisicao = "A operação requisitada é inválida.";
        internal const string ErroDuplicidade = "Não pode haver duplicidade de registros.";
        internal const string ErroExcecao = "Ocorreu um erro no servidor durante sua requisição.";
        internal const string ErroValidacao = "Por favor corrija os campos destacados.";
        internal const string AguardeOperacao = "Por favor aguarde a liberação do aprovador.";
        internal const string ErroChaveEstrangeira = "Não é possivel inativar o registro.";

        public static int ID = 1;

        public const string Administracao = "Administração";
        public const string Analista = "Analista";
        public const string Arquivo = "Arquivo";
    }
}
