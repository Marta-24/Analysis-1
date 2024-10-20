<?php
include 'db_connect.php'; 

$userId = $_POST["User_ID"];
$startSession = $_POST["Start_Session"];

// Añadir mensaje en los logs
error_log("Recibido session data: User_ID={$userId}, Start_Session={$startSession}");

$stmt = $conn->prepare("INSERT INTO `Sessions`(`UserID`, `StartSession`) VALUES (?, ?)");
if ($stmt === false) {
    error_log("Error en prepare(): " . $conn->error);
    die("Error en prepare(): " . $conn->error);
}

$stmt->bind_param("is", $userId, $startSession);

if ($stmt->execute()) {
    echo $conn->insert_id;
} else {
    error_log("Error en execute(): " . $stmt->error);
    echo "Error: " . $stmt->error;
}

$stmt->close();
$conn->close();
?>
