-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               10.0.25-MariaDB - SLE 12 SP1 package
-- Server OS:                    Linux
-- HeidiSQL Version:             9.3.0.5104
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;


-- Dumping database structure for vtsReaderDB
CREATE DATABASE IF NOT EXISTS `vtsReaderDB` /*!40100 DEFAULT CHARACTER SET utf8 */;
USE `vtsReaderDB`;

-- Dumping structure for table vtsReaderDB.logs
CREATE TABLE IF NOT EXISTS `logs` (
  `nginx_start` int(10) unsigned NOT NULL DEFAULT '0',
  `month` datetime NOT NULL,
  `zone` varchar(50) NOT NULL DEFAULT '0',
  `outBytes` bigint(20) unsigned NOT NULL DEFAULT '0',
  `inBytes` bigint(20) unsigned NOT NULL DEFAULT '0',
  `requests` bigint(20) unsigned NOT NULL,
  `updatedOn` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`month`,`zone`,`nginx_start`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Dumping data for table vtsReaderDB.logs: ~0 rows (approximately)
/*!40000 ALTER TABLE `logs` DISABLE KEYS */;
/*!40000 ALTER TABLE `logs` ENABLE KEYS */;

-- Dumping structure for table vtsReaderDB.meta
CREATE TABLE IF NOT EXISTS `meta` (
  `entity` varchar(50) NOT NULL,
  `value` varchar(50) DEFAULT NULL,
  UNIQUE KEY `key` (`entity`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Dumping data for table vtsReaderDB.meta: ~1 rows (approximately)
/*!40000 ALTER TABLE `meta` DISABLE KEYS */;
INSERT INTO `meta` (`entity`, `value`) VALUES
	('last_readings_year_month', '2016-09');
/*!40000 ALTER TABLE `meta` ENABLE KEYS */;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
