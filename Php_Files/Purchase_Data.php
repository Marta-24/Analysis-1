<?php
include 'db_connect.php'; 

$userId = $_POST["User_ID"];
$sessionId = $_POST["Session_ID"];
$itemId = $_POST["Item"];
$buyDate = $_POST["Buy_Date"];

// Añadir mensaje en los logs
error_log("Recibido purchase data: User_ID={$userId}, Session_ID={$sessionId}, Item={$itemId}, Buy_Date={$buyDate}");

$stmt = $conn->prepare("INSERT INTO `Purchases`(`UserID`, `SessionID`, `ItemID`, `BuyDate`) VALUES (?, ?, ?, ?)");
if ($stmt === false) {
    error_log("Error en prepare(): " . $conn->error);
    die("Error en prepare(): " . $conn->error);
}

$stmt->bind_param("iiis", $userId, $sessionId, $itemId, $buyDate);

if ($stmt->execute()) {
    echo $conn->insert_id;
} else {
    error_log("Error en execute(): " . $stmt->error);
    echo "Error: " . $stmt->error;
}

$stmt->close();
$conn->close();
?>
