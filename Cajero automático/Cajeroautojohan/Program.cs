using System;
using System.IO;
using System.Linq;
using System.Globalization;

class CajeroAutomatico
{
    // Archivos planos
    static string rutaUsuarios = "usuarios.txt";
    static string rutaMovimientos = "movimientos.txt";

    // Matriz de usuarios (ID, PIN, SALDO)
    static string[,] usuariosMatriz = new string[10, 3];

    // Variables de sesión
    static string idActivo = "";
    static double saldoActivo = 0;
    static int filaActiva = -1;

    static void Main()
    {
        InicializarArchivos();
        CargarUsuarios();

        if (AccesoCuenta())
        {
            bool salir = false;
            while (!salir)
            {
                MostrarMenu();
                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1": Depositar(); break;
                    case "2": Retirar(); break;
                    case "3": ConsultarSaldo(); break;
                    case "4": ConsultarHistorial(); break;
                    case "5": CambiarClave(); break;
                    case "6": salir = true; break;
                    default:
                        Console.WriteLine("\n Opción inválida.");
                        Console.ReadKey();
                        break;
                }
            }
        }
        else
        {
            Console.WriteLine("\n Acceso denegado. Programa terminado.");
        }
    }

    // ==============================
    // ARCHIVOS Y USUARIOS
    // ==============================
    static void InicializarArchivos()
    {
        if (!File.Exists(rutaUsuarios))
        {
            string[] usuariosDemo = {
                "1010;1234;500000",
                "2020;4321;250000",
                "3030;1111;100000"
            };
            File.WriteAllLines(rutaUsuarios, usuariosDemo);
        }

        if (!File.Exists(rutaMovimientos))
        {
            File.Create(rutaMovimientos).Close();
        }
    }

    static void CargarUsuarios()
    {
        string[] registros = File.ReadAllLines(rutaUsuarios);

        for (int i = 0; i < usuariosMatriz.GetLength(0); i++)
        {
            if (i < registros.Length)
            {
                string[] datos = registros[i].Split(';');
                if (datos.Length >= 3)
                {
                    usuariosMatriz[i, 0] = datos[0].Trim(); // ID
                    usuariosMatriz[i, 1] = datos[1].Trim(); // PIN
                    usuariosMatriz[i, 2] = datos[2].Trim(); // SALDO
                }
            }
        }
    }

    static void GuardarCambiosAArchivo()
    {
        string[] nuevasLineas = new string[usuariosMatriz.GetLength(0)];

        for (int i = 0; i < usuariosMatriz.GetLength(0); i++)
        {
            if (!string.IsNullOrWhiteSpace(usuariosMatriz[i, 0]))
            {
                nuevasLineas[i] = $"{usuariosMatriz[i, 0]};{usuariosMatriz[i, 1]};{usuariosMatriz[i, 2]}";
            }
        }

        File.WriteAllLines(rutaUsuarios, nuevasLineas.Where(l => l != null).ToArray());
    }

    // ==============================
    // LOGIN
    // ==============================
    static bool AccesoCuenta()
    {
        int intentos = 0;
        while (intentos < 3)
        {
            Console.Clear();
            DibujarCaja("Ingreso al Sistema");
            Console.Write("\n Ingrese ID de cuenta: ");
            string id = Console.ReadLine();
            Console.Write("Ingrese PIN: ");
            string pin = Console.ReadLine();

            for (int i = 0; i < usuariosMatriz.GetLength(0); i++)
            {
                if (usuariosMatriz[i, 0] == id && usuariosMatriz[i, 1] == pin)
                {
                    idActivo = usuariosMatriz[i, 0];
                    saldoActivo = double.Parse(usuariosMatriz[i, 2], CultureInfo.InvariantCulture);
                    filaActiva = i;

                    Console.WriteLine("\n Sesión iniciada con éxito.");
                    Console.ReadKey();
                    return true;
                }
            }

            intentos++;
            Console.WriteLine("\n Credenciales incorrectas. Intento {0}/3", intentos);
            Console.ReadKey();
        }
        return false;
    }

    // ==============================
    // MENÚ
    // ==============================
    static void MostrarMenu()
    {
        Console.Clear();
        DibujarCaja("BANCO VIRTUAL - MENÚ");
        Console.WriteLine("║ 1. Depósito                ║");
        Console.WriteLine("║ 2. Retiro                  ║");
        Console.WriteLine("║ 3. Consultar Saldo         ║");
        Console.WriteLine("║ 4. Ver Últimos Movimientos ║");
        Console.WriteLine("║ 5. Cambiar Clave           ║");
        Console.WriteLine("║ 6. Salir                   ║");
        Console.WriteLine("╚════════════════════════════╝");
        Console.Write("\nSeleccione una opción: ");
    }

    static void DibujarCaja(string titulo)
    {
        Console.WriteLine("╔════════════════════════════╗");
        Console.WriteLine($"║ {titulo.PadRight(28)}║");
        Console.WriteLine("╚════════════════════════════╝");
    }

    // ==============================
    // FUNCIONES DE CAJERO
    // ==============================
    static void Depositar()
    {
        Console.Clear();
        DibujarCaja("Depósito");
        Console.Write("Monto a depositar: ");
        if (double.TryParse(Console.ReadLine(), out double monto) && monto > 0)
        {
            saldoActivo += monto;
            ActualizarMatriz();
            GuardarMovimiento("Depósito", monto);
            Console.WriteLine($"\n Depósito de {monto} realizado.");
        }
        else
        {
            Console.WriteLine("\n Monto inválido.");
        }
        Console.ReadKey();
    }

    static void Retirar()
    {
        Console.Clear();
        DibujarCaja("Retiro");
        Console.WriteLine($"Saldo disponible: {saldoActivo}");
        Console.Write("Monto a retirar: ");
        if (double.TryParse(Console.ReadLine(), out double monto) && monto > 0)
        {
            if (monto <= saldoActivo)
            {
                saldoActivo -= monto;
                ActualizarMatriz();
                GuardarMovimiento("Retiro", monto);
                Console.WriteLine($"\n Retiro de {monto} exitoso.");
            }
            else
            {
                Console.WriteLine("\n Fondos insuficientes.");
            }
        }
        else
        {
            Console.WriteLine("\n Monto inválido.");
        }
        Console.ReadKey();
    }

    static void ConsultarSaldo()
    {
        Console.Clear();
        DibujarCaja("Consulta de Saldo");
        Console.WriteLine($"\n Su saldo actual es: {saldoActivo}");
        Console.ReadKey();
    }

    static void ConsultarHistorial()
    {
        Console.Clear();
        DibujarCaja("Últimos 5 Movimientos");
        var lista = File.ReadAllLines(rutaMovimientos)
                         .Where(linea => linea.Split(';')[0] == idActivo)
                         .Reverse()
                         .Take(5);

        if (!lista.Any())
        {
            Console.WriteLine("\n No hay movimientos registrados.");
        }
        else
        {
            foreach (var item in lista)
            {
                string[] datos = item.Split(';');
                Console.WriteLine($"\n Fecha: {datos[3]}\n Tipo: {datos[1]}\n Monto: {datos[2]}");
            }
        }
        Console.ReadKey();
    }

    static void CambiarClave()
    {
        Console.Clear();
        DibujarCaja("Cambio de Clave");
        Console.Write("Ingrese su clave actual: ");
        string claveActual = Console.ReadLine();

        if (claveActual == usuariosMatriz[filaActiva, 1])
        {
            Console.Write("Ingrese nueva clave: ");
            string nuevaClave = Console.ReadLine();

            usuariosMatriz[filaActiva, 1] = nuevaClave;
            GuardarCambiosAArchivo();
            Console.WriteLine("\n Clave cambiada exitosamente.");
        }
        else
        {
            Console.WriteLine("\n Clave incorrecta.");
        }
        Console.ReadKey();
    }

    // ==============================
    // AUXILIARES
    // ==============================
    static void ActualizarMatriz()
    {
        usuariosMatriz[filaActiva, 2] = saldoActivo.ToString(CultureInfo.InvariantCulture);
        GuardarCambiosAArchivo();
    }

    static void GuardarMovimiento(string tipo, double monto)
    {
        string linea = $"{idActivo};{tipo};{monto};{DateTime.Now}";
        File.AppendAllLines(rutaMovimientos, new[] { linea });
    }
}
