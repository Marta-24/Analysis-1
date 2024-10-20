<?php
include 'db_connect.php'; // Asegúrate de que este archivo conecta correctamente a la base de datos.

$name = $_POST["Name"];
$country = $_POST["Country"];
$age = $_POST["Age"];
$gender = $_POST["Gender"];
$dateOfCreation = $_POST["DateOfCreation"];

// Registrar en los logs los datos recibidos para verificación.
error_log("Recibido user info: Name={$name}, Country={$country}, Age={$age}, Gender={$gender}, DateOfCreation={$dateOfCreation}");

// Preparar la consulta para insertar los datos en usersInfo.
$stmt = $conn->prepare("INSERT INTO `usersInfo`(`Name`, `Country`, `Age`, `Gender`, `DateOfCreation`) VALUES (?, ?, ?, ?, ?)");

if ($stmt === false) {
    // Si la preparación de la consulta falla, registrar el error.
    error_log("Error en prepare(): " . $conn->error);
    die("Error en prepare(): " . $conn->error);
}

// Vincular parámetros (s -> string, i -> integer, d -> double).
$stmt->bind_param("ssids", $name, $country, $age, $gender, $dateOfCreation);

if ($stmt->execute()) {
    echo $conn->insert_id; // Devuelve el ID del nuevo usuario.
} else {
    error_log("Error en execute(): " . $stmt->error);
    echo "Error: " . $stmt->error;
}

$stmt->close();
$conn->close();
?>
