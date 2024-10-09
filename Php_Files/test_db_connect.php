<?php
include 'db_connect.php';

if ($conn->ping()) {
    echo "Conexión a la base de datos exitosa.";
} else {
    echo "Error al conectar a la base de datos: " . $conn->error;
}

$conn->close();
?>
