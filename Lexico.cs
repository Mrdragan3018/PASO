using System;
using System.Collections.Generic;

namespace CompiladorPASO
{
    public enum TokenType
    {
        IDENTIFICADOR,
        ENTERO,
        DECIMAL,
        CADENA, // texto (string)
        BOOLEANO, //booleano (bool)
        VERDADERO,
        FALSO,
        SUMA,
        RESTA,
        MULTIPLICACION,
        DIVISION,
        MAYOR_QUE, //>
        MENOR_QUE, //<
        MAYOR_IGUAL_QUE, //>=
        MENOR_IGUAL_QUE, //<=
        IGUAL_QUE, //==
        DIFERENTE_DE, //!=
        Y, //Y (&&)
        O, //O (||)
        COMA, //,
        PUNTO, //.
        NULO, //nulo (null)
        COMENTARIO, //~
        COMILLAS, //"
        IMPRIMIR, //muestra
        RETORNAR, //devuelve
        PARENTESIS_IZQ,
        PARENTESIS_DER,
        ASIGNACION,
        EOF, // Fin de archivo
        SI, //si (if)
        SINO, //si no (if else)
        NINGUNO, //ninguno antes (else)
        HAZ, //haz (do)
        MIENTRAS, //mientras (while)
        DURANTE, //siempre y cuando (for)
        INDENT,
        DEDENT,
        NUEVA_LINEA
    }

    public class Token
    {
        public TokenType Tipo { get; }
        public string Valor { get; }

        public Token(TokenType tipo, string valor)
        {
            Tipo = tipo;
            Valor = valor;
        }

        public override string ToString()
        {
            return $"Token(Tipo: {Tipo}, Valor: '{Valor}')";
        }
    }

    public class Lexer
    {
        private readonly string _codigoFuente;
        private int _posicion;
        private int _linea = 1;
        private int _columna = 1;
        private char _caracterActual;
        private int _nivelIndentacionActual = 0;
        private Stack<int> _nivelesIndentacion = new Stack<int>();
        private List<string> _errores;

        public Lexer(string codigoFuente)
        {
            _codigoFuente = codigoFuente;
            _posicion = 0;
            _caracterActual = _codigoFuente[_posicion];
            _errores = new List<string>();
        }

        private void Avanzar()
        {
            if ( _caracterActual == '\n' ) 
            {
                _linea++;
                _columna = 1;
            }
            else 
            {
                _columna++;
            }
            _posicion++;
            _caracterActual = _posicion < _codigoFuente.Length ? _codigoFuente[_posicion] : '\0';
        }

        private void AgregarError(string mensaje)
        {
            // Guarda el mensaje de error en la lista de errores
            _errores.Add($"Error en columna {_columna} de línea {_linea}: {mensaje}");
        }

        public List<Token> ObtenerTokens()
        {
            var tokens = new List<Token>();

            while (_caracterActual != '\0')
            {
                if (char.IsWhiteSpace(_caracterActual))
                {
                    if (_caracterActual == '\n')
                    {
                        tokens.Add(new Token(TokenType.NUEVA_LINEA, "\\n"));
                        Avanzar();
                        ManejarIndentacion(tokens);
                    }
                    else
                    {
                        Avanzar();
                    }
                    continue;
                }

                if (char.IsDigit(_caracterActual))
                {
                    tokens.Add(ObtenerNumero());
                    continue;
                }

                if (char.IsLetter(_caracterActual))
                {
                    tokens.Add(ObtenerIdentificadorOPalabraClave());
                    continue;
                }

                switch (_caracterActual)
                {
                    case '+':
                        tokens.Add(new Token(TokenType.SUMA, "+"));
                        Avanzar();
                        break;
                    case '-':
                        tokens.Add(new Token(TokenType.RESTA, "-"));
                        Avanzar();
                        break;
                    case '*':
                        tokens.Add(new Token(TokenType.MULTIPLICACION, "*"));
                        Avanzar();
                        break;
                    case '/':
                        tokens.Add(new Token(TokenType.DIVISION, "/"));
                        Avanzar();
                        break;
                    case '>':
                        tokens.Add(ObtenerOperadorComparacion('>', TokenType.MAYOR_QUE, TokenType.MAYOR_IGUAL_QUE));
                        break;
                    case '<':
                        tokens.Add(ObtenerOperadorComparacion('<', TokenType.MENOR_QUE, TokenType.MENOR_IGUAL_QUE));
                        break;
                    case '=':
                        tokens.Add(ObtenerOperadorComparacion('=', TokenType.ASIGNACION, TokenType.IGUAL_QUE));
                        break;
                    case '!':
                        Avanzar();
                        if (_caracterActual == '=')
                        {
                            tokens.Add(new Token(TokenType.DIFERENTE_DE, "!="));
                            Avanzar();
                        }
                        else
                        {
                            AgregarError("Símbolo de negación no válido.");
                        }
                        break;
                    case ',':
                        tokens.Add(new Token(TokenType.COMA, ","));
                        Avanzar();
                        break;
                    case '.':
                        tokens.Add(new Token(TokenType.PUNTO, "."));
                        Avanzar();
                        break;
                    case '"':
                        tokens.Add(ObtenerCadena());
                        break;
                    case '~':
                        Avanzar();
                        tokens.Add(ObtenerComentario());
                        break;
                    case '(':
                        tokens.Add(new Token(TokenType.PARENTESIS_IZQ, "("));
                        Avanzar();
                        break;
                    case ')':
                        tokens.Add(new Token(TokenType.PARENTESIS_DER, ")"));
                        Avanzar();
                        break;
                    default:
                        AgregarError($"Caracter no válido {_caracterActual}");
                        Avanzar();
                        break;
                }
            }

            tokens.Add(new Token(TokenType.EOF, string.Empty));
            if (_errores.Count > 0)
            {
                Console.WriteLine("--Errores léxicos encontrados--");
                foreach(var error in _errores) { Console.WriteLine(error); }
                Console.WriteLine("-------------------------------");
            }
            return tokens;
        }

        private Token ObtenerNumero()
        {
            string numero = string.Empty;
            while (char.IsDigit(_caracterActual))
            {
                numero += _caracterActual;
                Avanzar();
            }

            if (_caracterActual == '.')
            {
                numero += '.';
                Avanzar();

                while (char.IsDigit(_caracterActual))
                {
                    numero += _caracterActual;
                    Avanzar();
                }

                return new Token(TokenType.DECIMAL, numero);
            }

            return new Token(TokenType.ENTERO, numero);
        }

        private Token ObtenerIdentificadorOPalabraClave()
        {
            string texto = string.Empty;
            while (char.IsLetterOrDigit(_caracterActual))
            {
                texto += _caracterActual;
                Avanzar();
            }

            return texto switch
            {
                "Y" => new Token(TokenType.Y, "Y"),
                "O" => new Token(TokenType.O, "O"),
                /*"entero" => new Token(TokenType.ENTERO, "entero"),
                "decimal" => new Token(TokenType.DECIMAL, "decimal"),
                "texto" => new Token (TokenType.CADENA, "texto"),
                "booleano" => new Token (TokenType.BOOLEANO, "booleano"),*/
                "verdadero" => new Token(TokenType.VERDADERO, "verdadero"),
                "falso" => new Token(TokenType.FALSO, "falso"),
                "nulo" => new Token(TokenType.NULO, "nulo"),
                "si" => new Token(TokenType.SI, "si"),
                "si no" => new Token(TokenType.SINO, "si no"),
                "de lo contrario" => new Token(TokenType.NINGUNO, "de lo contrario"),
                "haz" => new Token(TokenType.HAZ, "haz"),
                "mientras" => new Token(TokenType.MIENTRAS, "mientras"),
                "durante" => new Token(TokenType.DURANTE, "durante"),
                "muestra" => new Token(TokenType.IMPRIMIR, "muestra"),
                "devuelve" => new Token(TokenType.RETORNAR, "devuelve"),
                _ => new Token(TokenType.IDENTIFICADOR, texto),
            };
        }

        private Token ObtenerOperadorComparacion(char operador, TokenType tipoSimple, TokenType tipoDoble)
        {
            Avanzar();
            if (_caracterActual == '=')
            {
                Avanzar();
                return new Token(tipoDoble, operador + "=");
            }
            return new Token(tipoSimple, operador.ToString());
        }

        private Token ObtenerCadena()
        {
            Avanzar();
            string cadena = string.Empty;
            while (_caracterActual != '"' && _caracterActual != '\0')
            {
                cadena += _caracterActual;
                Avanzar();
            }

            if (_caracterActual == '"') Avanzar(); 
            else AgregarError("Cadena no cerrada");
            return new Token(TokenType.CADENA, cadena);
        }

        private Token ObtenerComentario()
        {
            string comentario = string.Empty;
            while (_caracterActual != '\n' && _caracterActual != '\0')
            {
                comentario += _caracterActual;
                Avanzar();
            }
            return new Token(TokenType.COMENTARIO, comentario);
        }

        private void ManejarIndentacion(List<Token> tokens)
        {
            int numEspacios = 0;

            while (_caracterActual == ' ' || _caracterActual == '\t')
            {
                if (_caracterActual == ' ')
                    numEspacios++;
                Avanzar();
            }

            if (numEspacios > _nivelIndentacionActual)
            {
                tokens.Add(new Token(TokenType.INDENT, "INDENT"));
                _nivelesIndentacion.Push(numEspacios);
                _nivelIndentacionActual = numEspacios;
            }
            else if (numEspacios < _nivelIndentacionActual)
            {
                while (_nivelesIndentacion.Count > 0 && _nivelesIndentacion.Peek() > numEspacios)
                {
                    tokens.Add(new Token(TokenType.DEDENT, "DEDENT"));
                    _nivelesIndentacion.Pop();
                }
                _nivelIndentacionActual = numEspacios;
            }
        }
    }
}
