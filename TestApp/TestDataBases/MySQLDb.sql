CREATE DATABASE  IF NOT EXISTS `mysqldb` /*!40100 DEFAULT CHARACTER SET utf8 */;
USE `mysqldb`;
-- MySQL dump 10.13  Distrib 5.7.12, for Win64 (x86_64)
--
-- Host: localhost    Database: mysqldb
-- ------------------------------------------------------
-- Server version	5.7.15-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `course`
--

DROP TABLE IF EXISTS `course`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `course` (
  `Cid` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(20) DEFAULT NULL,
  `TeacherId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Cid`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `course`
--

LOCK TABLES `course` WRITE;
/*!40000 ALTER TABLE `course` DISABLE KEYS */;
INSERT INTO `course` VALUES (1,'Math',2),(2,'Chinese',1),(3,'English',3),(4,'P.E',1);
/*!40000 ALTER TABLE `course` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `learn`
--

DROP TABLE IF EXISTS `learn`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `learn` (
  `LearnId` int(11) NOT NULL AUTO_INCREMENT,
  `StudentId` int(11) DEFAULT NULL,
  `CourseId` int(11) DEFAULT NULL,
  `Grade` int(11) DEFAULT NULL,
  PRIMARY KEY (`LearnId`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `learn`
--

LOCK TABLES `learn` WRITE;
/*!40000 ALTER TABLE `learn` DISABLE KEYS */;
INSERT INTO `learn` VALUES (1,1,3,77),(2,1,2,88),(3,1,1,99),(4,2,1,66),(5,2,3,68),(6,2,2,100),(7,3,1,50),(8,3,2,78);
/*!40000 ALTER TABLE `learn` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `student`
--

DROP TABLE IF EXISTS `student`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `student` (
  `Sid` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(20) DEFAULT NULL,
  `Birthday` datetime DEFAULT NULL,
  PRIMARY KEY (`Sid`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `student`
--

LOCK TABLES `student` WRITE;
/*!40000 ALTER TABLE `student` DISABLE KEYS */;
INSERT INTO `student` VALUES (1,'Mike','1995-10-02 00:00:00'),(2,'Leo','1996-04-02 00:00:00'),(3,'Jack','1997-05-03 00:00:00');
/*!40000 ALTER TABLE `student` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `teacher`
--

DROP TABLE IF EXISTS `teacher`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `teacher` (
  `Tid` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(20) DEFAULT NULL,
  PRIMARY KEY (`Tid`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `teacher`
--

LOCK TABLES `teacher` WRITE;
/*!40000 ALTER TABLE `teacher` DISABLE KEYS */;
INSERT INTO `teacher` VALUES (1,'Mr.Li'),(2,'Mis.Swift'),(3,'Mis.Alice');
/*!40000 ALTER TABLE `teacher` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping events for database 'mysqldb'
--

--
-- Dumping routines for database 'mysqldb'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2018-10-02 23:32:16
