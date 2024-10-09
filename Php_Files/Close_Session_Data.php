<?php
include 'db_connect.php';

$sessionId = $_POST["Session_ID"];
$endSession = $_POST["End_Session"];

error_log("Received end session data: Session_ID={$sessionId}, End_Session={$endSession}");

$stmt = $conn->prepare("UPDATE `Sessions` SET `endSession` = ? WHERE `sessionId` = ?");
$stmt->bind_param("si", $endSession, $sessionId);

if ($stmt->execute()) {
    if ($stmt->affected_rows > 0) {
        //echo "Session closed successfully";
        echo $endSession;
    } else {
        error_log("No session updated in Close_Session_Data.php");
        echo "No session updated";
    }
} else {
    error_log("Error in Close_Session_Data.php: " . $stmt->error);
    echo "Error: " . $stmt->error;
}

$stmt->close();
$conn->close();
?>
