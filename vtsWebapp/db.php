<?php

// Server related variables stored at nginx_vts_stats.php (outside www root)
include('/opt/conf/nginx_vts_stats.php');

try {
	$conn = new PDO("mysql:host=$servername;dbname=$dbname", $username, $password);
	$conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);	

	$stmt = $conn->prepare("select zone, month, sum(outBytes) as `out`, sum(inBytes) as `in`, sum(requests) as `requests` from logs
		where month >= (:start_month) AND month <= (:end_month)						
		group by TRIM(LEADING 'www.' from zone)
		ORDER BY `out` DESC
		;");

	$start_month = $_GET["start_month"] . '-01 00:00:00';
	$end_month = null;
	if($_GET["end_month"] == '') {
		$end_month = $start_month;	
	} else {

		$end_month = $_GET["end_month"] . '-01 00:00:00';


	}

	$stmt->bindValue(':start_month', $start_month);
	$stmt->bindValue(':end_month', $end_month);
	$stmt->execute();


	// set the resulting array to associative
	$result = $stmt->fetchAll();

	header('Content-type: application/json');
	print json_encode($result);

}
catch(PDOException $e) {
	echo "Error: " . $e->getMessage();
}

$conn = null;

?>