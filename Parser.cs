using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;

namespace CompiladorPASO
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _indiceActual;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _indiceActual = 0;
        }

        public void Parsear()
        {
            while (!EsFinDeArchivo())
            {
                ParsearInstruccion();
            }
        }

        private void ParsearInstruccion()
        {
            var tokenActual = ObtenerTokenActual();

            switch (tokenActual.Tipo)
            {
                case TokenType.SI:
                    ParsearCondicional();
                    break;
                case TokenType.MIENTRAS:
                    ParsearMientras();
                    break;
                case TokenType.HAZ:
                    ParsearHazMientras();
                    break;
                case TokenType.IMPRIMIR:
                    ParsearImprimir();
                    break;
                case TokenType.IDENTIFICADOR:
                    ParsearAsignacion();
                    break;
                case TokenType.ENTERO: // Suponiendo que INT es un tipo de dato, como int en otros lenguajes
                case TokenType.DECIMAL:
                    ParsearDeclaracionVariable();
                    break;
                default:
                    throw new Exception($"Instrucción no reconocida: {tokenActual.Tipo}");
            }
        }

        private void ParsearCondicional()
        {
            // Consumes 'SI'
            Consumir(TokenType.SI);
            Consumir(TokenType.PARENTESIS_IZQ);
            // Parsea la condición de tipo booleano
            var condicion = ParsearExpresion();
            Consumir(TokenType.PARENTESIS_DER);
            // Indentación obligatoria en el bloque
            Consumir(TokenType.INDENT);

            // Parsea el bloque de código
            while (!EsToken(TokenType.DEDENT) && !EsFinDeArchivo())
            {
                ParsearInstruccion();
            }

            Consumir(TokenType.DEDENT);

            // Parsea opcionalmente 'SINO' o 'NINGUNO'
            if (EsToken(TokenType.SINO))
            {
                Consumir(TokenType.SINO);
                Consumir(TokenType.INDENT);
                while (!EsToken(TokenType.DEDENT) && !EsFinDeArchivo())
                {
                    ParsearInstruccion();
                }
                Consumir(TokenType.DEDENT);
            }
            else if (EsToken(TokenType.NINGUNO))
            {
                Consumir(TokenType.NINGUNO);
                Consumir(TokenType.INDENT);
                while (!EsToken(TokenType.DEDENT) && !EsFinDeArchivo())
                {
                    ParsearInstruccion();
                }
                Consumir(TokenType.DEDENT);
            }

        }
         
        private void ParsearMientras()
        {
            Consumir(TokenType.MIENTRAS);
            Consumir(TokenType.PARENTESIS_IZQ);
            var condicion = ParsearExpresion();
            Consumir(TokenType.PARENTESIS_DER);
            Consumir(TokenType.INDENT);

            while (!EsToken(TokenType.DEDENT) && !EsFinDeArchivo())
            {
                ParsearInstruccion();
            }

            Consumir(TokenType.DEDENT);
        }

        private void ParsearHazMientras()
        {
            Consumir(TokenType.HAZ);
            Consumir(TokenType.INDENT);

            do
            {
                ParsearInstruccion();
            }
            while (!EsToken(TokenType.DEDENT) && !EsFinDeArchivo());

            Consumir(TokenType.DEDENT);
            Consumir(TokenType.MIENTRAS);
            Consumir(TokenType.PARENTESIS_IZQ);
            var condicion = ParsearExpresion();
            Consumir(TokenType.PARENTESIS_DER);
        }

        private void ParsearImprimir()
        {
            Consumir(TokenType.IMPRIMIR);
            Consumir(TokenType.PARENTESIS_IZQ);
            var expresion = ParsearExpresion();
            Consumir(TokenType.PARENTESIS_DER);

            string impresion = expresion.ToString();

            Console.WriteLine(impresion);

            // Generar lógica para 'imprimir' en el entorno deseado.
        }

        private void ParsearAsignacion()
        {
            //var nombreTipo = Consumir(TokenType.ASIGNACION);
            var nombreVariable = Consumir(TokenType.IDENTIFICADOR);
            Consumir(TokenType.ASIGNACION);
            var expresion = ParsearExpresion();
            // Validar tipo y asignar valor.
        }

        private Token ParsearExpresion()
        {
            var token = ObtenerTokenActual();

            // Ejemplo básico: si es un número entero o decimal
            if (token.Tipo == TokenType.ENTERO || token.Tipo == TokenType.DECIMAL)
            {
                Consumir(token.Tipo); // Avanzamos después de procesar
                return token;
            }

            // Ejemplo de identificación de una operación matemática
            if (token.Tipo == TokenType.IDENTIFICADOR)
            {
                Consumir(TokenType.IDENTIFICADOR);
                // Puedes agregar aquí la lógica de operaciones
            }

            throw new Exception("Expresión inválida");
  
        }

        private void ParsearDeclaracionVariable()
        {
            // Primer token debería ser el tipo de la variable (INT, FLOAT, etc.)
            var tipoVariable = Consumir(ObtenerTokenActual().Tipo);

            // El siguiente token debe ser el identificador de la variable
            var nombreVariable = Consumir(TokenType.IDENTIFICADOR);

            // Opcionalmente, se puede inicializar la variable en la misma declaración
            if (EsToken(TokenType.ASIGNACION))
            {
                Consumir(TokenType.ASIGNACION);
                var valorInicial = ParsearExpresion();
                // Aquí podrías guardar la variable con su tipo y valor en el entorno
            }

            // Agrega la variable a una tabla de símbolos u otra estructura de almacenamiento si es necesario
            Console.WriteLine($"Variable declarada: Tipo {tipoVariable}, Nombre {nombreVariable}");
        }


        private Token Consumir(TokenType tipoEsperado)
        {
            if (ObtenerTokenActual().Tipo == tipoEsperado)
            {
                _indiceActual++;
            }
            else
            {
                throw new Exception($"Se esperaba token de tipo {tipoEsperado}, pero se encontró {ObtenerTokenActual().Tipo}");
            }
            return _tokens[_indiceActual++];
        }

        private bool EsToken(TokenType tipo)
        {
            return ObtenerTokenActual().Tipo == tipo;
        }

        private Token ObtenerTokenActual()
        {
            return _indiceActual < _tokens.Count ? _tokens[_indiceActual] : new Token(TokenType.EOF, "");
        }

        private bool EsFinDeArchivo()
        {
            return ObtenerTokenActual().Tipo == TokenType.EOF;
        }
    }
}

/*using System;
using System.Collections.Generic;

namespace CompiladorPASO
{
    public class Parser
    {
        private List<Token> tokens;
        private int posicionActual;
        private Dictionary<string, object> variables;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
            this.posicionActual = 0;
            this.variables = new Dictionary<string, object>();
        }

        private Token TokenActual => tokens[posicionActual];
        private Token Avanzar() => tokens[posicionActual++];
        private bool Coincidir(TokenType tipo)
        {
            if (TokenActual.Tipo == tipo)
            {
                Avanzar();
                return true;
            }
            return false;
        }

        public void Parsear()
        {
            while (TokenActual.Tipo != TokenType.EOF)
            {
                if (Coincidir(TokenType.IMPRIMIR))
                {
                    ParsearImprimir();
                }
                else if (Coincidir(TokenType.SI))
                {
                    ParsearCondicionalSI();
                }
                else if (Coincidir(TokenType.IDENTIFICADOR))
                {
                    ParsearDeclaracionOAsignacion();
                }
                else if (Coincidir(TokenType.MIENTRAS))
                {
                    ParsearMientras();
                }
                else if (Coincidir(TokenType.NUEVA_LINEA) || Coincidir(TokenType.INDENT) || Coincidir(TokenType.DEDENT))
                {
                    Avanzar();
                }
                else
                {
                    Errores($"Instrucción desconocida: {TokenActual.Valor}");
                }
            }
        }

        private void ParsearImprimir()
        {
            if (Coincidir(TokenType.PARENTESIS_IZQ))
            {
                Token valorImpresion = Avanzar();

                if (Coincidir(TokenType.PARENTESIS_DER))
                {
                    Console.WriteLine(valorImpresion.Valor);
                }
                else
                {
                    Errores("Se esperaba un paréntesis de cierre.");
                }
            }
            else
            {
                Errores("Se esperaba un paréntesis de apertura para 'IMPRIMIR'.");
            }
        }

        private void ParsearCondicionalSI()
        {
            if (Coincidir(TokenType.PARENTESIS_IZQ))
            {
                bool resultadoCondicion = EvaluarExpresion();

                if (Coincidir(TokenType.PARENTESIS_DER))
                {
                    if (Coincidir(TokenType.NUEVA_LINEA))
                    {
                        if (Coincidir(TokenType.INDENT))
                        {
                            if (resultadoCondicion)
                            {
                                ParsearBloque();
                            }
                            else
                            {
                                SaltarBloque();
                            }
                        }
                    }
                    else { Errores("Se esperaba salto de línea antes del bloque de código"); }
                }
                else { Errores("Se esperaba paréntesis de cierre después de la condición"); }
            }
            else { Errores("Se esperaba paréntesis de apertura para la condición"); }
        }

        private void ParsearMientras()
        {
            if (Coincidir(TokenType.PARENTESIS_IZQ))
            {
                bool resultadoCondicion = EvaluarExpresion();

                if (Coincidir(TokenType.PARENTESIS_DER))
                {
                    if (Coincidir(TokenType.NUEVA_LINEA))
                    {
                        if (Coincidir(TokenType.INDENT))
                        {
                            while (resultadoCondicion)
                            {
                                ParsearBloque();
                                resultadoCondicion = EvaluarExpresion(); // Re-evaluar la condición
                            }
                            SaltarBloque();
                        }
                    }
                    else { Errores("Se esperaba salto de línea antes del bloque de código"); }
                }
                else { Errores("Se esperaba paréntesis de cierre después de la condición"); }
            }
            else { Errores("Se esperaba paréntesis de apertura para la condición"); }
        }

        private bool EvaluarExpresion()
        {
            Token expr = Avanzar();
            if (expr.Tipo == TokenType.BOOLEANO)
            {
                return expr.Valor.ToString() == "VERDADERO";
            }
            else if (expr.Tipo == TokenType.ENTERO)
            {
                return int.Parse(expr.Valor.ToString()) > 0;
            }
            else
            {
                Errores("Expresión no válida.");
                return false;
            }
        }

        private void ParsearDeclaracionOAsignacion()
        {
            string nombreVariable = TokenActual.Valor.ToString();
            Avanzar();

            if (Coincidir(TokenType.ASIGNACION))
            {
                object valor = EvaluarExpresion();
                variables[nombreVariable] = valor;
            }
            else
            {
                Errores("Se esperaba un operador de asignación '='.");
            }
        }

        private void SaltarBloque()
        {
            while (TokenActual.Tipo != TokenType.DEDENT && TokenActual.Tipo != TokenType.EOF)
            {
                Avanzar();
            }
        }

        private void ParsearBloque()
        {
            while (TokenActual.Tipo != TokenType.DEDENT && TokenActual.Tipo != TokenType.EOF)
            {
                Parsear();
                if (TokenActual.Tipo == TokenType.NUEVA_LINEA)
                {
                    Avanzar();
                }
            }

            if (TokenActual.Tipo == TokenType.DEDENT)
            {
                Avanzar();
            }
            else
            {
                Errores("Se esperaba un DEDENT para cerrar el bloque de código.");
            }
        }

        private void Errores(string mensaje)
        {
            throw new Exception(mensaje);
        }
    }
}*/

/*using System;
using System.Collections.Generic;

namespace CompiladorPASO
{
    public class Parser
    {
        private List<Token> tokens;
        private int posicionActual;

        public Parser(List<Token> tokens) { this.tokens = tokens; }

        private Token TokenActual => tokens[posicionActual];
        private Token Avanzar() => tokens[posicionActual++];

        private bool Coincidir(TokenType tipo)
        {
            if (TokenActual.Tipo == tipo)
            {
                Avanzar();
                return true;
            }
            return false;
        }
        public void Parsear()
        {
            while (TokenActual.Tipo != TokenType.EOF)
            {
                if (Coincidir(TokenType.IMPRIMIR))
                {
                    ParsearImprimir();
                }
                else if (Coincidir(TokenType.SI))
                {
                    ParsearCondicionalSI();
                }
                else if (Coincidir(TokenType.IDENTIFICADOR))
                {
                    ParsearDeclaracionOAsignacion();
                }
                else if (Coincidir(TokenType.MIENTRAS))
                {
                    ParsearMientras();
                }
                else if (Coincidir(TokenType.COMENTARIO))
                {
                    while (TokenActual.Tipo != TokenType.NUEVA_LINEA && TokenActual.Tipo != TokenType.EOF)
                    {
                        Avanzar();
                    }
                }
                else if (Coincidir(TokenType.NUEVA_LINEA) || Coincidir(TokenType.INDENT) || Coincidir(TokenType.DEDENT))
                {
                    Avanzar();
                }
                else
                {
                    Errores($"Instrucción desconocida: {TokenActual.Valor}");
                }
            }
        }

        private void ParsearImprimir()
        {
            Token valorImpresion = Avanzar();

            if (Avanzar().Tipo == TokenType.PARENTESIS_IZQ)
            {
                if (valorImpresion.Tipo == TokenType.BOOLEANO || valorImpresion.Tipo == TokenType.CADENA || valorImpresion.Tipo == TokenType.ENTERO || valorImpresion.Tipo == TokenType.DECIMAL)
                {
                    Token impresion = valorImpresion;
                    if (Avanzar().Tipo == TokenType.PARENTESIS_DER)
                    {
                        Console.WriteLine(valorImpresion.Valor);
                    }
                    else { Errores("Se esperaba paréntesis de cierre"); }
                }
                else
                {
                    Errores("Introduzca un tipo de dato valido");
                }
            }
            else
            {
                Errores("Se eperaba paréntesis de apertura");
            }
        }

        //Parseado de los condicionales
        private void ParsearCondicionalSI()
        {
            Avanzar();

            // Verifica apertura de paréntesis '('
            if (Coincidir(TokenType.PARENTESIS_IZQ))
            {
                Avanzar();
                bool resultadoCondicion = EvaluarExpresion(); // Evalúa la expresión de condición

                // Verifica cierre de paréntesis ')'
                if (Coincidir(TokenType.PARENTESIS_DER))
                {
                    Avanzar();
                    // Espera un salto de línea antes del bloque
                    if (Coincidir(TokenType.NUEVA_LINEA))
                    {
                        Avanzar();

                        // Verifica indentación para el bloque de código
                        if (Coincidir(TokenType.INDENT))
                        {
                            Avanzar();

                            // Ejecuta el bloque si la condición es verdadera
                            if (resultadoCondicion)
                            {
                                ParsearBloque();
                            }
                            else
                            {
                                SaltarBloque(); // Evita ejecutar el bloque si la condición es falsa
                            }
                        }
                        // Verificación del "SINO" (sin indentación adicional)
                        else if (Coincidir(TokenType.SINO))
                        {
                            Avanzar();
                            ParsearCondicionalSI();
                        }
                        // Verificación del "NINGUNO" (sin indentación adicional)
                        else if (Coincidir(TokenType.NINGUNO))
                        {
                            Avanzar();
                            ParsearCondicionalDeLoContrario(); // Maneja 'else'
                        }
                        else { Errores("Se esperaba indentado antes del bloque de código"); }
                    }
                    else { Errores("Se esperaba salto de línea antes del bloque de código"); }
                }
                else { Errores(@"Se esperaba paréntesis de cierre después de la condición"); }
            }
            else { Errores(@"Se esperaba paréntesis de apertura ""("" antes de la condición"); }
        }

        // Maneja el bloque "NINGUNO" (else) sin indentado
        private void ParsearCondicionalDeLoContrario()
        {
            Avanzar();

            // Espera salto de línea antes del bloque de código
            if (Coincidir(TokenType.NUEVA_LINEA))
            {
                Avanzar();
                if (Coincidir(TokenType.INDENT)) // Aquí espera indentado solo en el bloque
                {
                    ParsearBloque(); // Ejecuta el bloque "else"
                }
                else { Errores("Se esperaba indentado antes del bloque de código"); }
            }
            else { Errores("Se esperaba salto de línea antes del bloque de código"); }
        }
        private void SaltarBloque()
        {
            // Avanza hasta encontrar un DEDENT o el final del archivo
            while (TokenActual.Tipo != TokenType.DEDENT && TokenActual.Tipo != TokenType.EOF)
            {
                Avanzar();
            }
        }
        private void ParsearBloque()
        {
            // Procesa múltiples declaraciones dentro del bloque hasta el DEDENT o EOF
            while (TokenActual.Tipo != TokenType.DEDENT && TokenActual.Tipo != TokenType.EOF)
            {
                Parsear(); // Procesa cada declaración
                if (TokenActual.Tipo == TokenType.NUEVA_LINEA)
                {
                    Avanzar(); // Avanza después de cada nueva línea dentro del bloque
                }
            }

            // Verifica el cierre del bloque con un DEDENT
            if (TokenActual.Tipo == TokenType.DEDENT)
            {
                Avanzar(); // Avanza después del DEDENT para continuar parseando fuera del bloque
            }
            else
            {
                Errores("Se esperaba un DEDENT para cerrar el bloque de código.");
            }
        }


        //Manejo de errores
        private void Errores(string mensaje)
        {
            throw new Exception(mensaje);
        }
    }
}*/
