CREATE TABLE Categoria (
	ID INT IDENTITY PRIMARY KEY,
	Nome NVARCHAR(50) NOT NULL,
)

CREATE TABLE Cor (
	ID INT IDENTITY PRIMARY KEY,
	Nome NVARCHAR(50) NOT NULL,
	Hexadecimal NVARCHAR(50) NOT NULL
)

CREATE TABLE Tamanho (
	ID INT IDENTITY PRIMARY KEY,
	Nome NVARCHAR(50) NOT NULL,
)

CREATE TABLE Produto (
	ID INT IDENTITY PRIMARY KEY,
	Nome NVARCHAR(50) NOT NULL,
	Descricao NVARCHAR(255) NOT NULL,
	Valor DECIMAL(10,2) NOT NULL,
	Status INT NOT NULL DEFAULT 1
);

CREATE TABLE [Produto.Cor] (
	ID INT IDENTITY PRIMARY KEY,

	ProdutoID INT NOT NULL,
	CONSTRAINT FK_Produto FOREIGN KEY(ProdutoID) REFERENCES Produto(ID),

	CorID INT NOT NULL,
	CONSTRAINT FK_ProdutoCor FOREIGN KEY(CorID) REFERENCES Cor(ID),

	Status INT NOT NULL
);

CREATE TABLE [Produto.Tamanho] (
	ID INT IDENTITY PRIMARY KEY,

	ProdutoID INT NOT NULL,
	CONSTRAINT FK_PDProduto FOREIGN KEY(ProdutoID) REFERENCES Produto(ID),

	TamanhoID INT NOT NULL,
	CONSTRAINT FK_ProdutoTamanho FOREIGN KEY(TamanhoID) REFERENCES Tamanho(ID),

	Status INT NOT NULL
);

CREATE TABLE [Produto.Imagem] (
	ID INT IDENTITY PRIMARY KEY,

	CorID INT NOT NULL,
	CONSTRAINT FK_ImagemProdutoCor FOREIGN KEY(CorID) REFERENCES Cor(ID),

	ProdutoID INT NOT NULL,
	CONSTRAINT FK_ImagemProduto FOREIGN KEY(ProdutoID) REFERENCES Produto(ID),

	ImageURL NVARCHAR(255) NOT NULL,

	Status INT NOT NULL
);

CREATE TABLE [Produto.Categoria] (
	ID INT IDENTITY PRIMARY KEY,

	ProdutoID INT NOT NULL,
	CONSTRAINT FK_CategoriaProduto FOREIGN KEY(ProdutoID) REFERENCES Produto(ID),

	CategoriaID INT NOT NULL,
	CONSTRAINT FK_ProdutoCategoria FOREIGN KEY(CategoriaID) REFERENCES Categoria(ID),

	Status INT NOT NULL
);

CREATE TABLE Estoque (
	ID INT IDENTITY PRIMARY KEY,

	ProdutoID INT NOT NULL,
	CONSTRAINT FK_EstoqueProduto FOREIGN KEY(ProdutoID) REFERENCES Produto(ID),

	Quantidade INT NOT NULL,
	Valor DECIMAL(10,2) NOT NULL,
);

CREATE TABLE Cliente (
	ID INT IDENTITY PRIMARY KEY,
	Nome VARCHAR(255) NOT NULL,
	Email VARCHAR(255) NOT NULL,
	DataNasc DATE NOT NULL,
	Celular VARCHAR(14) NOT NULL,
	Valor DECIMAL(10,2) NOT NULL,
)

CREATE TABLE Pedido (
	ID INT IDENTITY PRIMARY KEY,

	UsuarioID INT NOT NULL,
	CONSTRAINT FK_Usuario FOREIGN KEY(UsuarioID) REFERENCES Usuario(UsuarioID),

	DataPedido DATE NOT NULL,
	DataPagamento DATE NULL,
	DataEntrega DATE NULL,
	QuantidadeTotal INT NULL,
	ValorTotal DECIMAL(10,2) NULL,
	Frete DECIMAL(10,2) NULL,
	StatusPedido INT NOT NULL,
)


CREATE TABLE [Pedido.Item] (
	ID INT IDENTITY PRIMARY KEY,

	PedidoID INT NOT NULL,
	CONSTRAINT FK_Pedido_Item FOREIGN KEY(PedidoID) REFERENCES Pedido(ID),

	ProdutoID INT NOT NULL,
	CONSTRAINT FK_Produto_Item FOREIGN KEY(ProdutoID) REFERENCES Produto(ID),

	Cor NVARCHAR(50) NULL,
	ImageURL NVARCHAR(255) NULL,
	Tamanho NVARCHAR(50) NOT NULL,
	Quantidade INT NOT NULL,
	ValorTotal DECIMAL(10,2) NOT NULL,
);

CREATE TABLE Movimentacao (
	ID INT IDENTITY PRIMARY KEY,
	Descricao NVARCHAR(50) NOT NULL,
	DataEmissao DATE NOT NULL,
	NF VARCHAR(11) NOT NULL,
	Tipo INT NOT NULL,
	Conta NVARCHAR(50) NOT NULL,
	Vencimento DATE NOT NULL,
	DataPagamento DATE NOT NULL,
	Valor DECIMAL(10,2) NOT NULL,
	Status INT NOT NULL DEFAULT 0,
);

/********************************************************************/
/************************************************************* USER */
/********************************************************************/

CREATE TABLE Pais (
	PaisID INT IDENTITY PRIMARY KEY,	
	Nome NVARCHAR(50) NOT NULL,
	NomePT NVARCHAR(50)NULL,
	Sigla NVARCHAR(3) NOT NULL,

	Status INT NOT NULL DEFAULT 1,
	RegistradoEm SMALLDATETIME NOT NULL DEFAULT GETDATE(),
	RegistradoPorUsuarioID INT NULL,
	AtualizadoEm SMALLDATETIME NOT NULL DEFAULT GETDATE(),
	AtualizadoPorUsuarioID INT NULL,
)

CREATE TABLE [Pais.Estado] (
	EstadoID INT IDENTITY PRIMARY KEY,
	Nome NVARCHAR(50) NOT NULL,
	Sigla NVARCHAR(3) NOT NULL,

	PaisID INT NOT NULL,
	CONSTRAINT FK_Estado_Pais FOREIGN KEY (PaisID) REFERENCES Pais(PaisID),

	Status INT NOT NULL DEFAULT 1,
	RegistradoEm SMALLDATETIME NOT NULL DEFAULT GETDATE(),
	RegistradoPorUsuarioID INT NULL,
	AtualizadoEm SMALLDATETIME NOT NULL DEFAULT GETDATE(),
	AtualizadoPorUsuarioID INT NULL
)

CREATE TABLE [Pais.Estado.Cidade] (
	CidadeID INT IDENTITY PRIMARY KEY,
	
	EstadoID INT NOT NULL,
	CONSTRAINT FK_Cidade_Estado FOREIGN KEY (EstadoID) REFERENCES [Pais.Estado](EstadoID),

	PaisID INT NOT NULL,
	CONSTRAINT FK_Cidade_Pais FOREIGN KEY (PaisID) REFERENCES Pais(PaisID),

	Nome NVARCHAR(50) NOT NULL,

	Status INT NOT NULL DEFAULT 1,
	RegistradoEm SMALLDATETIME NOT NULL DEFAULT GETDATE(),
	RegistradoPorUsuarioID INT NULL,
	AtualizadoEm SMALLDATETIME NOT NULL DEFAULT GETDATE(),
	AtualizadoPorUsuarioID INT NULL
)

CREATE TABLE Perfil (
	ID INT IDENTITY PRIMARY KEY,
	Nome NVARCHAR(50) NOT NULL,
	Status INT NOT NULL DEFAULT 1
);

CREATE TABLE [Perfil.Acao] (
	AcaoID INT IDENTITY PRIMARY KEY,
	
	PerfilID INT NOT NULL,
	CONSTRAINT FK_Acao_Perfil FOREIGN KEY(PerfilID) REFERENCES Perfil(ID),

	Nome NVARCHAR(50) NOT NULL,
	Grupo NVARCHAR(50),
	Valor NVARCHAR(100),
	Habilitado BIT NOT NULL DEFAULT 0,

	Status INT NOT NULL DEFAULT 1,
	RegistradoEm SMALLDATETIME NOT NULL DEFAULT GETDATE(),
	RegistradoPor INT NULL,
	AtualizadoEm SMALLDATETIME NOT NULL DEFAULT GETDATE(),
	AtualizadoPor INT NULL
)

CREATE TABLE Usuario (
	UsuarioID INT IDENTITY PRIMARY KEY,
	
	PerfilID INT NOT NULL,
	CONSTRAINT FK_Perfil FOREIGN KEY(PerfilID) REFERENCES Perfil(ID),

	CPF NVARCHAR(11) NOT NULL,
	Email NVARCHAR(50) NOT NULL,
	Senha NVARCHAR(50) NOT NULL,
	Nome NVARCHAR(50) NOT NULL,
	IsAdmin BIT NOT NULL DEFAULT 0,
	Celular NVARCHAR(20) NOT NULL,
	DataNascimento DATE NOT NULL,
	ImageURL NVARCHAR(255) NULL,

	ResetSenhaChave NVARCHAR(50),
	ResetSenhaDataLimite SMALLDATETIME,
	ResetSenhaLogin BIT NOT NULL DEFAULT 0,

	RegistradoEm SMALLDATETIME NOT NULL DEFAULT GETDATE(),
	RegistradoPor INT NULL,
	AtualizadoEm SMALLDATETIME NOT NULL DEFAULT GETDATE(),
	AtualizadoPor INT NULL
);

CREATE TABLE [Usuario.Perfil] (
	UsuarioPerfilID INT IDENTITY PRIMARY KEY,
	
	UsuarioID INT NULL,
	CONSTRAINT FK_Usuario_Perfil FOREIGN KEY(UsuarioID) REFERENCES Usuario(UsuarioID),

	PerfilID INT NULL,
	CONSTRAINT FK_Perfil_Usuario FOREIGN KEY(PerfilID) REFERENCES Perfil(ID)
);

 CREATE TABLE [Usuario.Endereco](
	UsuarioEnderecoID INT IDENTITY PRIMARY KEY,

	PaisID INT NOT NULL,
	CONSTRAINT FK_UsuarioEndereco_Pais FOREIGN KEY (PaisID) REFERENCES Pais(PaisID),

	EstadoID INT NOT NULL,
	CONSTRAINT FK_UsuarioEndereco_Estado FOREIGN KEY (EstadoID) REFERENCES [Pais.Estado](EstadoID),

	CidadeID INT NOT NULL,
	CONSTRAINT FK_UsuarioEndereco_Cidade FOREIGN KEY (CidadeID) REFERENCES [Pais.Estado.Cidade](CidadeID),

	UsuarioID INT NOT NULL,
	CONSTRAINT FK_Endereco_Usuario FOREIGN KEY(UsuarioID) REFERENCES Usuario(UsuarioID),

	Logradouro NVARCHAR(50) NULL,
	Numero NVARCHAR(6) NULL,
	Complemento NVARCHAR(50) NULL,
	Bairro NVARCHAR(30) NULL,
	CEP NVARCHAR(9) NULL
);

INSERT INTO Perfil (Nome, Status) VALUES ('Administrador', 1);
INSERT INTO Usuario (PerfilID, CPF, Email, Senha, Nome, Celular, DataNascimento) VALUES (1, '46952821864', 'jhon@jhon.com.br', 'Kd6PNHquAI7gClGHsdR0WQ==', 'jhon', '11993785589', '18-01-98');
INSERT INTO Usuario (PerfilID, CPF, Email, Senha, Nome, Celular, DataNascimento, IsAdmin) VALUES (1, '46952821864', 'admin@admin.com.br', 'Kd6PNHquAI7gClGHsdR0WQ==', 'admin', '11993785589', '18-01-98', 1);
INSERT INTO [Usuario.Perfil] (UsuarioID, PerfilID) VALUES (6, 1);


--DROP
--drop table Estoque
--drop table ItemPedido
--drop table [Produto.Tamanho]
--drop table [Produto.Cor]
--drop table [Produto.Imagem]
--drop table [Produto.Categoria]
--drop table Produto
--drop table Cor
--drop table Categoria
--drop table Pedido
--drop table [Pedido.Item]