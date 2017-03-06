/*
Navicat MySQL Data Transfer

Source Server         : localhost
Source Server Version : 50505
Source Host           : localhost:3306
Source Database       : tcp-moe

Target Server Type    : MYSQL
Target Server Version : 50505
File Encoding         : 65001

Date: 2017-03-06 14:31:34
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `products`
-- ----------------------------
DROP TABLE IF EXISTS `products`;
CREATE TABLE `products` (
  `name` varchar(32) CHARACTER SET utf8 NOT NULL,
  `description` varchar(512) CHARACTER SET utf8 NOT NULL DEFAULT '-',
  `process` varchar(64) NOT NULL,
  `dllPath` varchar(64) CHARACTER SET utf8 NOT NULL,
  `aesKey` varchar(128) CHARACTER SET utf8 NOT NULL,
  `available` tinyint(4) NOT NULL DEFAULT '1'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of products
-- ----------------------------
INSERT INTO `products` VALUES ('moe CS:GO', 'Coder: aequabit', 'csgo.exe', 'moeCSGO.dll', 'RM3#2p@M^<0OoB9VLI&a0ig[Yc7b_yZ:', '1');
INSERT INTO `products` VALUES ('moe CS:S', 'Coder: aequabit', 'hl2.exe', 'moeCSS.dll', 'xBcv6x,HYQqbD.l:4@i($)fJjMR9#&Fg', '1');

-- ----------------------------
-- Table structure for `users`
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(24) CHARACTER SET utf8 NOT NULL,
  `password` varchar(256) CHARACTER SET utf8 NOT NULL,
  `hwid` varchar(1024) CHARACTER SET utf8 DEFAULT NULL,
  `products` varchar(1024) CHARACTER SET utf8 NOT NULL DEFAULT '{}',
  `rank` varchar(32) DEFAULT NULL,
  `status` varchar(16) CHARACTER SET utf8 NOT NULL DEFAULT 'unverified',
  `created_at` timestamp NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of users
-- ----------------------------
INSERT INTO `users` VALUES ('1', 'aequabit', 'ayy', null, '{\"moe CS:GO\":1496343600, \"moe CS:S\":1518123100}', 'Administrator', 'verified', '2017-03-06 14:31:16', '2017-03-06 14:31:16');
