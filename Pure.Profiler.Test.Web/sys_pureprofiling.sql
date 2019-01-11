/*
Navicat MySQL Data Transfer

Source Server         : mysql-192.168.6.52_AGRYGL
Source Server Version : 50721
Source Host           : 192.168.6.52:3306
Source Database       : agrygl

Target Server Type    : MYSQL
Target Server Version : 50721
File Encoding         : 65001

Date: 2019-01-11 17:42:37
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for sys_pureprofiling
-- ----------------------------
DROP TABLE IF EXISTS `sys_pureprofiling`;
CREATE TABLE `sys_pureprofiling` (
  `SEQ` varchar(50) NOT NULL,
  `MachineName` varchar(120) DEFAULT NULL,
  `Type` varchar(50) DEFAULT NULL,
  `Id` varchar(50) DEFAULT NULL,
  `ParentId` varchar(50) DEFAULT NULL,
  `NAME` text,
  `Started` datetime DEFAULT NULL,
  `StartMilliseconds` bigint(20) DEFAULT NULL,
  `DurationMilliseconds` bigint(20) DEFAULT NULL,
  `Tags` text,
  `Sort` bigint(20) DEFAULT NULL,
  `DATA` text,
  `SessionId` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`SEQ`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
