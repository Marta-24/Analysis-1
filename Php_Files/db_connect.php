<?php
$servername = "localhost:3306";
$username = "albertcf5";
$password = "48103884m";
$database = "albertcf5"; 

// Create connection
$conn = new mysqli($servername, $username, $password, $database);

// Check connection
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
}
?>
