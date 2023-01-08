using System;
using System.Collections.Generic;
using System.Text;

namespace EstiloMB.Core
{
    public partial class Text
    {
        public const string Sucesso = "Sucesso.";
        public const string AcessoNegado = "Acesso negado.";
        public const string ErroValidacao = "Erro de validação.";
        public const string ErroRequisicao = "Erro de requisição.";
        public const string ErroServidor = "Internal Server Error";
        public const string ErroChaveEstrangeira = "Não é possivel excluir, pois o registra está sendo utilizado em outra tela";
        public const string Log = "Log";

        public const string Login = "Login realizado com sucesso.";

        #region [Permissoes]
        public const string Aprovador = "Aprovador";
        public const string NaoAprovador = "Não Aprovador";
        public const string Administrador = "Administrador";
        public const string Componentes = "Componentes";

        #endregion

        #region [Perfil]
        internal const string Perfis = "Perfis";
        internal const string PerfilAdicionado = "{0} criou o perfil {1}.";
        internal const string PerfilAtualizado = "{0} atualizou o perfil {1}.";
        internal const string PerfilRemovido = "{0} removeu o perfil {1}.";
        internal const string PerfilNomeJaRegistrado = "Este nome já está registrado.";
        internal const string PerfilNaoPodeSalvar = "Este perfil não pode ser alterado.";
        internal const string PerfilNaoPodeRemover = "Este perfil não pode ser removido.";
        #endregion

        #region [Login]
        internal const string TelaLogin = "Tela Login";
        internal const string TelaLoginFormatoInvalido = "Formato de imagem inválido.";
        internal const string LoginTelaAtualizada = "{0} atualizou a tela de login.";
        #endregion

        #region [Usuario]
        internal const string Usuarios = "Usuários";
        internal const string UsuarioAdicionado = "{0} criou o usuário {1}.";
        internal const string UsuarioAtualizado = "{0} atualizou o usuário {1}.";
        internal const string UsuarioRemovido = "{0} removeu o usuário {1}.";
        internal const string UsuarioEmailJaRegistrado = "Este e-mail já está registrado.";
        internal const string UsuarioNaoPodeSalvar = "Este usuário não pode ser alterado.";
        internal const string UsuarioNaoPodeRemover = "Este usuário não pode ser removido.";
        internal const string UsuarioNaoEncontrado = "Usuário incorreto ou não encontrado";
        internal const string UsuarioBloqueado = "Esta conta de usuário foi bloqueada.";
        internal const string UsuarioSemAcesso = "Este usuário não tem acesso ao sistema";
        internal const string UsuarioSenhaIncorreta = "A senha está incorreta.";
        internal const string UsuarioRequerTrocaSenha = "Este usuário foi marcado para uma troca de senha obrigatória.";
        internal const string UsuarioPrazoExpirou = "A requisição para redefinição de senha expirou.";
        internal const string SenhaNaoPodeSerIgualAnterior = "A nova senha não pode ser igual à senha anterior.";
        #endregion

        #region [Categoria]
        internal const string Categoria = "Categoria";
        internal const string CategoriaNomeJaRegistrado = "Esta Categoria Ja foi Registrada";
        internal const string CategoriaCriada = "Categoria criada com sucesso";
        internal const string CategoriaEmUso = "Esta categoria está sendo utilizada";

        #endregion

        #region [Cor]
        internal const string Cor = "Cor";
        internal const string CorNomeJaRegistrado = "Esta Cor Ja foi Registrada";
        internal const string CorCriada = "Cor criada com sucesso";
        internal const string CorEmUso = "Esta cor está sendo utilizada";
        #endregion

        #region [Produto]
        internal const string Produto = "Produto";
        internal const string ProdutoNomeJaRegistrado = "Este Produto Ja foi Registrado";
        internal const string ProdutoCriado = "ProdutoCriado";
        #endregion

    }
}
