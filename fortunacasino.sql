-- phpMyAdmin SQL Dump
-- version 5.1.1
-- https://www.phpmyadmin.net/
--
-- Gép: 127.0.0.1
-- Létrehozás ideje: 2026. Ápr 14. 00:29
-- Kiszolgáló verziója: 10.4.20-MariaDB
-- PHP verzió: 7.3.29

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Adatbázis: `fortunacasino`
--
CREATE DATABASE IF NOT EXISTS `fortunacasino` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `fortunacasino`;

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `lotterydraws`
--

CREATE TABLE `lotterydraws` (
  `Id` bigint(20) NOT NULL,
  `DrawDate` datetime(6) NOT NULL,
  `TicketPrice` decimal(10,2) NOT NULL,
  `WinningNumbers` longtext DEFAULT NULL,
  `IsDrawn` tinyint(1) NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `TotalTicketsSold` int(11) NOT NULL,
  `TotalPayout` decimal(10,2) NOT NULL,
  `DrawnAt` datetime(6) DEFAULT NULL,
  `DrawnBy` bigint(20) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `GameType` varchar(20) NOT NULL DEFAULT 'Lottery5'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- A tábla adatainak kiíratása `lotterydraws`
--

INSERT INTO `lotterydraws` (`Id`, `DrawDate`, `TicketPrice`, `WinningNumbers`, `IsDrawn`, `IsActive`, `TotalTicketsSold`, `TotalPayout`, `DrawnAt`, `DrawnBy`, `CreatedAt`, `GameType`) VALUES
(1, '2026-05-05 20:04:01.000000', '400.00', NULL, 0, 1, 0, '0.00', NULL, NULL, '2026-03-03 23:44:41.000000', 'Lottery5'),
(2, '2026-05-05 20:04:01.000000', '300.00', NULL, 0, 1, 0, '0.00', NULL, NULL, '2026-03-03 23:44:41.000000', 'Lottery6'),
(3, '2026-05-05 20:04:01.000000', '200.00', NULL, 0, 1, 0, '0.00', NULL, NULL, '2026-03-03 23:44:41.000000', 'Keno'),
(4, '2026-05-05 20:04:01.000000', '450.00', NULL, 0, 1, 0, '0.00', NULL, NULL, '2026-04-04 10:40:36.000000', 'Scandinavian'),
(5, '2026-05-05 20:04:01.000000', '800.00', NULL, 0, 1, 0, '0.00', NULL, NULL, '2026-04-04 10:40:36.000000', 'Eurojackpot'),
(6, '2026-05-05 20:04:01.000000', '300.00', NULL, 0, 1, 0, '0.00', NULL, NULL, '2026-04-04 10:40:36.000000', 'Joker'),
(7, '2026-04-11 09:36:50.162000', '300.00', NULL, 0, 0, 0, '0.00', NULL, NULL, '2026-04-11 09:32:38.772252', 'ötös');

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `lotterytickets`
--

CREATE TABLE `lotterytickets` (
  `Id` bigint(20) NOT NULL,
  `UserId` bigint(20) NOT NULL,
  `DrawId` bigint(20) NOT NULL,
  `TicketCode` varchar(255) NOT NULL,
  `FieldsNumbers` longtext DEFAULT NULL,
  `Fields` int(11) DEFAULT NULL,
  `FieldsFilled` tinyint(3) UNSIGNED NOT NULL,
  `IsQuickPick` tinyint(1) NOT NULL,
  `TotalPrice` decimal(10,2) NOT NULL,
  `MatchesNumbers` tinyint(3) UNSIGNED DEFAULT NULL,
  `Matches` tinyint(3) UNSIGNED DEFAULT NULL,
  `TotalWinAmount` decimal(10,2) NOT NULL,
  `IsPaidOut` tinyint(1) NOT NULL,
  `Status` longtext NOT NULL,
  `BoughtAt` datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- A tábla adatainak kiíratása `lotterytickets`
--

INSERT INTO `lotterytickets` (`Id`, `UserId`, `DrawId`, `TicketCode`, `FieldsNumbers`, `Fields`, `FieldsFilled`, `IsQuickPick`, `TotalPrice`, `MatchesNumbers`, `Matches`, `TotalWinAmount`, `IsPaidOut`, `Status`, `BoughtAt`) VALUES
(1, 9, 1, 'LOT260300001', '3;4;6;26;30', 1, 1, 1, '400.00', 0, NULL, '0.00', 0, 'drawn', '2026-03-11 16:39:35.458703'),
(2, 9, 2, 'LOT260300002', '16;18;25;78;85', 1, 1, 1, '300.00', 0, NULL, '0.00', 0, 'drawn', '2026-03-11 16:39:45.699972'),
(3, 9, 3, 'LOT260300003', '18;19;28;31;83', 1, 1, 1, '200.00', 0, NULL, '0.00', 0, 'drawn', '2026-03-11 16:39:51.404806'),
(4, 9, 1, 'LOT260300004', '3;19;26;55;82', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-03-11 17:11:17.859374'),
(5, 9, 2, 'LOT260300005', '13;29;42;56;59', 1, 1, 1, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-03-11 17:19:21.551938'),
(6, 9, 3, 'LOT260300006', '26;29;42;74;78', 1, 1, 1, '200.00', NULL, NULL, '0.00', 0, 'active', '2026-03-11 17:19:27.879335'),
(7, 9, 1, 'LOT260300007', '5;12;23;34;45', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '0001-01-01 00:00:00.000000'),
(8, 9, 1, 'LOT260300008', '6;13;22;31;44', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '0001-01-01 00:00:00.000000'),
(37, 15, 1, 'UWGUTV17OELY', '12;23;34;45;56', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:49:56.245147'),
(38, 15, 2, '3SRNUMNQ7EES', '5;15;25;35;45;55', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:50:11.485552'),
(39, 15, 3, 'BJB5WOC241Y3', '2;8;15;23;31;40;47;55;62;70', 1, 1, 0, '200.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:50:26.065337'),
(40, 15, 4, 'EOJWSVBHAVNZ', '7;14;21;28;35;42;49', 1, 1, 0, '450.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:50:40.753935'),
(41, 15, 5, '1BAVA2J92EQK', '3;5;10;12;19;28;37', 1, 1, 0, '800.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:50:58.587739'),
(42, 15, 6, '3CZV9THCY3XY', '123456', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:51:11.513829'),
(43, 15, 6, 'C04BP99EVR04', '123456', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:51:11.519308'),
(44, 15, 6, 'EQD954JT96EL', '784512', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:52:50.084712'),
(45, 15, 6, 'F2OJQLNJ1AHF', '784512', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:52:50.088475'),
(46, 15, 6, '5QK5UIAILYVK', '123456', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:53:15.107255'),
(47, 15, 3, 'K5UOFN91FXW6', '2;8;15;23;31;40;47;55;62;70', 1, 1, 0, '200.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:53:15.112661'),
(48, 15, 1, 'YDQFCY1GHVXD', '10;20;30;40;50', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:53:47.238666'),
(49, 15, 4, '79AN9F7T6X6N', '7;14;21;28;35;42;49', 1, 1, 0, '450.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:53:47.242627'),
(50, 15, 5, 'Z5NU6RBPTPCI', '3;5;10;12;19;28;37', 1, 1, 0, '800.00', NULL, NULL, '0.00', 0, 'active', '2026-04-04 08:53:47.246084'),
(51, 12, 1, 'LGSK2ERU6IF1', '5;12;20;60;87', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 17:29:06.493610'),
(52, 12, 6, 'I4HMAD3H1Q1D', '205061', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 17:29:06.557755'),
(53, 12, 6, 'KPF1OKO2Q458', '427115', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 17:29:06.564970'),
(54, 12, 6, 'CK4AA5P7E06W', '819483', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 17:29:06.569049'),
(55, 12, 1, 'B122I3TFCZ3I', '35;39;51;67;74', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 17:50:44.991522'),
(56, 12, 1, 'AH2J6RCA26L7', '1;7;27;48;84', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 17:50:45.053185'),
(57, 12, 1, '4MRQTI8LBGNF', '52;56;57;59;84', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 17:50:45.058809'),
(58, 12, 1, 'QV9A6TY46TLX', '14;40;53;74;89', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 17:58:31.046505'),
(59, 12, 6, 'NGZBNLAXFYWK', '871419', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 17:58:31.191445'),
(60, 12, 6, 'S8G79FUGDOE8', '797010', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 17:58:31.197612'),
(61, 12, 6, 'MU4JLUSLE5BH', '053010', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 17:58:31.202721'),
(62, 12, 1, '8ZY234TZY9KX', '6;19;30;35;76', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:05:18.716172'),
(63, 12, 1, 'BBOHAGHTZLQB', '1;4;12;22;86', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:05:39.095940'),
(64, 12, 1, 'FQ9F7MDXCEOW', '11;53;56;59;65', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:05:39.104468'),
(65, 12, 1, '8XFO44KVNCN1', '12;22;24;32;46', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:10:38.552049'),
(66, 12, 1, '5YFY5NGNQLZS', '12;42;69;71;90', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:10:38.556372'),
(67, 12, 1, '361IYD3CEPLC', '24;37;41;56;70', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:11:23.049871'),
(68, 12, 1, 'MTSLL3S5Z910', '15;50;60;74;77', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:11:23.055703'),
(69, 12, 6, '84RZW8ABK721', '538281', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:11:23.061561'),
(70, 12, 6, 'BL6WIZ2N3FMP', '637249', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:11:23.067082'),
(71, 12, 6, 'ZAYEWOAAAFM8', '027117', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:11:23.072292'),
(72, 12, 1, 'MB8IMR10ERBP', '7;18;29;31;59', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:49:47.423900'),
(73, 12, 6, 'A8RXSYL2LB0U', '573548', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:49:47.457553'),
(74, 12, 6, 'JSR3GOLBY823', '656820', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:49:47.462542'),
(75, 12, 6, 'B4M0DVZSMZ2Y', '627372', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-05 18:49:47.468281'),
(76, 9, 1, 'ZMLVM6C4LRTP', '3;8;14;50;59', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 17:16:23.933334'),
(77, 9, 1, 'RDISQYNZ0CR7', '2;3;58;84;85', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 17:16:23.972941'),
(78, 9, 1, 'YGKVI7DFKLIC', '7;8;33;53;88', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 17:16:23.978004'),
(79, 9, 1, 'GTM97FIIO064', '50;60;64;73;78', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 17:16:23.988840'),
(80, 9, 1, 'D3OLNV9DH4AC', '26;40;46;51;77', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 17:16:23.993056'),
(81, 9, 1, 'KNJB4BLRK411', '6;26;42;77;79', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 17:16:23.996655'),
(82, 9, 6, 'KCFAKXKH2AUS', '352136', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 17:16:24.000104'),
(83, 9, 6, 'W8B89DW39C56', '311529', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 17:16:24.004422'),
(84, 9, 6, 'MBRAPGVJMY41', '939978', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 17:16:24.008680'),
(85, 12, 1, '9P1A0S5YQB76', '47;48;58;74;84', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.097377'),
(86, 12, 1, '7VJEQ1N3SBC7', '52;76;77;80;89', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.132099'),
(87, 12, 1, 'W3SZOIX8MB2H', '3;5;17;42;46', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.136230'),
(88, 12, 1, '8HX7ZBJISED2', '18;36;56;61;70', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.140576'),
(89, 12, 1, 'Z6T7DVF8Q8VN', '2;3;18;30;32', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.144992'),
(90, 12, 1, '8B5XIYB7BV54', '11;18;45;50;68', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.149197'),
(91, 12, 6, 'FBWFH73QUWDR', '519410', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.154035'),
(92, 12, 6, '32J78UPAIWSB', '149195', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.158105'),
(93, 12, 6, 'ZUN4JP7BVR5Y', '628872', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.161655'),
(94, 12, 3, 'EWA6ZF73LK0O', '30;40;49;77;87', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.165082'),
(95, 12, 3, 'GVE4L62LRYSX', '46;52;54;57;65', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.168513'),
(96, 12, 3, '49FSVRO4FU4V', '3;4;20;29;49', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.172722'),
(97, 12, 5, 'GOZUXA6NHN72', '1;4;48;61;75', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.176804'),
(98, 12, 5, 'WMBA0CHY0D0Z', '12;16;21;45;82', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.180334'),
(99, 12, 5, 'IZW0B3Z8Z5DZ', '3;25;35;58;68', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.184070'),
(100, 12, 5, 'LR651MIK1O4F', '1;46;55;75;90', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.189567'),
(101, 12, 5, '75PHHWJNW6N0', '2;53;57;75;90', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.194221'),
(102, 12, 5, '5S2NATO5YPBR', '2;8;9;19;39', 1, 1, 1, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-06 18:00:02.198274'),
(103, 16, 6, 'BEBA5MZT1T8L', '551103', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-07 09:33:42.336255'),
(104, 16, 6, 'P64K4PZGBB56', '605063', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-07 09:33:42.422462'),
(105, 16, 6, '72DY85JZJHRP', '050393', 1, 1, 0, '300.00', NULL, NULL, '0.00', 0, 'active', '2026-04-07 09:33:42.427775'),
(106, 16, 1, 'CNJ9VPKJW3MJ', '2;7;33;53;58', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-07 09:33:42.431831'),
(107, 16, 1, 'BES51I8ICUDW', '46;50;75;82;86', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-07 09:33:42.436207'),
(108, 16, 1, '6LWXI44ATUXZ', '9;17;18;26;70', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-07 09:33:42.440786'),
(109, 16, 1, 'YR19VORBDCZV', '48;56;68;72;82', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-07 09:33:42.445465'),
(110, 16, 1, 'ZUNLSE5SJ3DH', '30;32;42;50;52', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-07 09:33:42.449551'),
(111, 16, 1, '1370X7IM5IZ0', '46;56;57;58;86', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-07 09:33:42.454409'),
(112, 16, 1, 'P4ZS198S57J9', '4;5;44;46;86', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-07 16:19:17.764779'),
(113, 9, 1, 'LNFIROAJ58WY', '7;34;57;70;83', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-07 16:27:41.748037'),
(114, 9, 1, 'Q8BESHBWCUKU', '3;56;66;79;89', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-07 16:27:41.759681'),
(115, 9, 1, '6HAGBQCPV4KX', '9;31;48;49;74', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-07 16:55:50.844253'),
(116, 9, 1, 'G701JCG6PT64', '2;22;33;41;70', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-10 20:39:54.691364'),
(117, 9, 1, 'SPAX1ARC8HB6', '1;7;31;46;65', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-10 20:39:54.769679'),
(118, 9, 1, 'TPPH91LCMGHZ', '14;59;61;66;72', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-10 20:39:54.775577'),
(119, 9, 1, '6DHZDVF0Q5BQ', '8;13;19;45;53', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-10 20:39:54.779512'),
(120, 9, 1, 'HITF10QNL44Z', '25;28;31;69;74', 1, 1, 0, '400.00', NULL, NULL, '0.00', 0, 'active', '2026-04-10 20:39:54.784424');

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `roles`
--

CREATE TABLE `roles` (
  `Id` tinyint(3) UNSIGNED NOT NULL,
  `RoleName` longtext NOT NULL,
  `Description` longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- A tábla adatainak kiíratása `roles`
--

INSERT INTO `roles` (`Id`, `RoleName`, `Description`) VALUES
(1, 'user', NULL),
(2, 'admin', NULL);

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `transactions`
--

CREATE TABLE `transactions` (
  `Id` bigint(20) NOT NULL,
  `UserId` bigint(20) NOT NULL,
  `Type` longtext NOT NULL,
  `Amount` decimal(10,2) NOT NULL,
  `BalanceBefore` decimal(10,2) NOT NULL,
  `BalanceAfter` decimal(10,2) NOT NULL,
  `TicketId` bigint(20) DEFAULT NULL,
  `Description` longtext DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- A tábla adatainak kiíratása `transactions`
--

INSERT INTO `transactions` (`Id`, `UserId`, `Type`, `Amount`, `BalanceBefore`, `BalanceAfter`, `TicketId`, `Description`, `CreatedAt`) VALUES
(1, 9, 'ticket_purchase', '-400.00', '100000.00', '99600.00', NULL, 'Szelvény: LOT260300001', '2026-03-11 16:39:35.549785'),
(2, 9, 'ticket_purchase', '-300.00', '99600.00', '99300.00', NULL, 'Szelvény: LOT260300002', '2026-03-11 16:39:45.706682'),
(3, 9, 'ticket_purchase', '-200.00', '99300.00', '99100.00', NULL, 'Szelvény: LOT260300003', '2026-03-11 16:39:51.410270'),
(4, 9, 'ticket_purchase', '-400.00', '99100.00', '98700.00', NULL, 'Szelvény: LOT260300004', '2026-03-11 17:11:17.987882'),
(5, 9, 'ticket_purchase', '-300.00', '98700.00', '98400.00', NULL, 'Szelvény: LOT260300005', '2026-03-11 17:19:21.558210'),
(6, 9, 'ticket_purchase', '-200.00', '98400.00', '98200.00', NULL, 'Szelvény: LOT260300006', '2026-03-11 17:19:27.883742'),
(7, 9, 'ticket_purchase', '-400.00', '98200.00', '97800.00', NULL, 'Szelvény: LOT260300007 (1 mező)', '2026-03-11 18:12:45.013545'),
(8, 9, 'ticket_purchase', '-400.00', '97800.00', '97400.00', NULL, 'Szelvény: LOT260300008 (1 mező)', '2026-03-11 18:17:02.709320'),
(9, 9, 'demo_topup', '1000.00', '97400.00', '98400.00', NULL, 'Demo feltöltés: +1000 Ft', '2026-03-16 19:53:23.716965'),
(10, 9, 'demo_topup', '10000.00', '98400.00', '108400.00', NULL, 'Demo feltöltés: +10000 Ft', '2026-03-23 23:18:03.383734'),
(11, 9, 'ticket_purchase', '-400.00', '108400.00', '108000.00', NULL, 'Szelvény: LOT260300009', '2026-03-23 23:34:58.252999'),
(12, 9, 'ticket_purchase', '-800.00', '108000.00', '107200.00', NULL, 'Szelvény: LOT260300010', '2026-03-23 23:35:49.043134'),
(13, 9, 'ticket_purchase', '-1200.00', '107200.00', '106000.00', NULL, 'Szelvény: LOT260300011', '2026-03-23 23:36:00.709692'),
(14, 9, 'ticket_purchase', '-500.00', '106000.00', '105500.00', NULL, '1 szelvény vásárlása', '2026-04-04 08:23:02.201327'),
(15, 9, 'ticket_purchase', '-650.00', '105500.00', '104850.00', NULL, '1 szelvény vásárlása', '2026-04-04 08:23:46.155476'),
(16, 9, 'ticket_purchase', '-800.00', '104850.00', '104050.00', NULL, '1 szelvény vásárlása', '2026-04-04 08:24:10.334639'),
(17, 15, 'ticket_purchase', '-800.00', '10000.00', '9200.00', NULL, '1 szelvény vásárlása', '2026-04-04 08:25:34.649930'),
(18, 15, 'ticket_purchase', '-300.00', '9200.00', '8900.00', NULL, '1 szelvény vásárlása', '2026-04-04 08:30:47.402870'),
(19, 15, 'ticket_purchase', '-600.00', '8900.00', '8300.00', NULL, '2 szelvény vásárlása', '2026-04-04 08:35:34.757731'),
(20, 15, 'ticket_purchase', '-700.00', '8300.00', '7600.00', NULL, '2 szelvény vásárlása', '2026-04-04 08:35:58.532047'),
(21, 15, 'ticket_purchase', '-400.00', '7600.00', '7200.00', NULL, '1 szelvény vásárlása', '2026-04-04 08:49:56.295619'),
(22, 15, 'ticket_purchase', '-300.00', '7200.00', '6900.00', NULL, '1 szelvény vásárlása', '2026-04-04 08:50:11.486800'),
(23, 15, 'ticket_purchase', '-200.00', '6900.00', '6700.00', NULL, '1 szelvény vásárlása', '2026-04-04 08:50:26.065735'),
(24, 15, 'ticket_purchase', '-450.00', '6700.00', '6250.00', NULL, '1 szelvény vásárlása', '2026-04-04 08:50:40.754279'),
(25, 15, 'ticket_purchase', '-800.00', '6250.00', '5450.00', NULL, '1 szelvény vásárlása', '2026-04-04 08:50:58.588065'),
(26, 15, 'ticket_purchase', '-600.00', '5450.00', '4850.00', NULL, '2 szelvény vásárlása', '2026-04-04 08:51:11.519552'),
(27, 15, 'ticket_purchase', '-600.00', '4850.00', '4250.00', NULL, '2 szelvény vásárlása', '2026-04-04 08:52:50.088588'),
(28, 15, 'ticket_purchase', '-500.00', '4250.00', '3750.00', NULL, '2 szelvény vásárlása', '2026-04-04 08:53:15.112869'),
(29, 15, 'ticket_purchase', '-1650.00', '3750.00', '2100.00', NULL, '3 szelvény vásárlása', '2026-04-04 08:53:47.246246'),
(30, 12, 'ticket_purchase', '-1300.00', '10000.00', '8700.00', NULL, '4 szelvény vásárlása', '2026-04-05 17:29:06.570312'),
(31, 12, 'ticket_purchase', '-1200.00', '8700.00', '7500.00', NULL, '3 szelvény vásárlása', '2026-04-05 17:50:45.060130'),
(32, 12, 'ticket_purchase', '-1300.00', '7500.00', '6200.00', NULL, '4 szelvény vásárlása', '2026-04-05 17:58:31.204580'),
(33, 12, 'ticket_purchase', '-400.00', '6200.00', '5800.00', NULL, '1 szelvény vásárlása', '2026-04-05 18:05:18.788784'),
(34, 12, 'ticket_purchase', '-800.00', '5800.00', '5000.00', NULL, '2 szelvény vásárlása', '2026-04-05 18:05:39.104838'),
(35, 12, 'ticket_purchase', '-800.00', '5000.00', '4200.00', NULL, '2 szelvény vásárlása', '2026-04-05 18:10:38.556597'),
(36, 12, 'ticket_purchase', '-1700.00', '4200.00', '2500.00', NULL, '5 szelvény vásárlása', '2026-04-05 18:11:23.072570'),
(37, 9, 'demo_topup', '100000.00', '104050.00', '204050.00', NULL, 'Demo feltöltés: +100000 Ft', '2026-04-05 18:49:07.238245'),
(38, 12, 'ticket_purchase', '-1300.00', '2500.00', '1200.00', NULL, '4 szelvény vásárlása', '2026-04-05 18:49:47.468749'),
(39, 12, 'deposit', '10000.00', '1200.00', '11200.00', NULL, 'Egyenleg feltöltés: +10 000 Ft', '2026-04-05 21:51:14.196047'),
(40, 9, 'deposit', '200000.00', '204050.00', '404050.00', NULL, 'Egyenleg feltöltés: +200 000 Ft', '2026-04-05 21:52:11.666785'),
(41, 9, 'deposit', '1000.00', '404050.00', '405050.00', NULL, 'Egyenleg feltöltés: +1 000 Ft', '2026-04-06 16:57:31.439810'),
(42, 9, 'deposit', '1000.00', '405050.00', '406050.00', NULL, 'Egyenleg feltöltés: +1 000 Ft', '2026-04-06 16:58:18.797629'),
(43, 9, 'deposit', '5000.00', '406050.00', '411050.00', NULL, 'Egyenleg feltöltés: +5 000 Ft', '2026-04-06 17:00:11.558492'),
(44, 9, 'deposit', '5000.00', '411050.00', '416050.00', NULL, 'Egyenleg feltöltés: +5 000 Ft', '2026-04-06 17:15:24.049932'),
(45, 9, 'deposit', '2000.00', '416050.00', '418050.00', NULL, 'Egyenleg feltöltés: +2 000 Ft', '2026-04-06 17:15:37.005813'),
(46, 9, 'ticket_purchase', '-3300.00', '418050.00', '414750.00', NULL, '9 szelvény vásárlása', '2026-04-06 17:16:24.008909'),
(47, 9, 'deposit', '101.00', '414750.00', '414851.00', NULL, 'Egyenleg feltöltés: +101 Ft', '2026-04-06 17:26:29.169827'),
(48, 9, 'deposit', '1000.00', '414851.00', '415851.00', NULL, 'Egyenleg feltöltés: +1 000 Ft', '2026-04-06 17:26:57.451141'),
(49, 9, 'deposit', '1000.00', '415851.00', '416851.00', NULL, 'Egyenleg feltöltés: +1 000 Ft', '2026-04-06 17:30:37.607411'),
(50, 9, 'deposit', '1000.00', '416851.00', '417851.00', NULL, 'Egyenleg feltöltés: +1 000 Ft', '2026-04-06 17:31:46.056235'),
(51, 9, 'deposit', '1000.00', '417851.00', '418851.00', NULL, 'Egyenleg feltöltés: +1 000 Ft', '2026-04-06 17:35:22.499495'),
(52, 9, 'deposit', '1000.00', '418851.00', '419851.00', NULL, 'Egyenleg feltöltés: +1 000 Ft', '2026-04-06 17:39:56.586748'),
(53, 9, 'deposit', '1000.00', '419851.00', '420851.00', NULL, 'Egyenleg feltöltés: +1 000 Ft', '2026-04-06 17:45:07.475256'),
(54, 9, 'deposit', '5000.00', '420851.00', '425851.00', NULL, 'Egyenleg feltöltés: +5 000 Ft', '2026-04-06 17:57:03.945962'),
(55, 12, 'deposit', '10000.00', '11200.00', '21200.00', NULL, 'Egyenleg feltöltés: +10 000 Ft', '2026-04-06 17:57:30.614083'),
(56, 12, 'ticket_purchase', '-6900.00', '21200.00', '14300.00', NULL, '18 szelvény vásárlása', '2026-04-06 18:00:02.198486'),
(57, 9, 'deposit', '2000.00', '425851.00', '427851.00', NULL, 'Egyenleg feltöltés: +2 000 Ft', '2026-04-06 23:13:58.946166'),
(58, 15, 'admin_topup', '10000.00', '2100.00', '12100.00', NULL, 'Admin feltöltés (admin: 9): +10 000 Ft', '2026-04-06 23:31:57.429508'),
(59, 9, 'admin_topup', '5000.00', '427851.00', '432851.00', NULL, 'Admin feltöltés (admin: 9): +5 000 Ft', '2026-04-06 23:32:14.116887'),
(60, 15, 'admin_topup', '5000.00', '12100.00', '17100.00', NULL, 'Admin feltöltés (admin: 9): +5 000 Ft', '2026-04-07 07:08:59.355070'),
(61, 16, 'deposit', '20000.00', '10000.00', '30000.00', NULL, 'Egyenleg feltöltés: +20 000 Ft', '2026-04-07 09:22:25.603345'),
(62, 16, 'ticket_purchase', '-3300.00', '30000.00', '26700.00', NULL, '9 szelvény vásárlása', '2026-04-07 09:33:42.455601'),
(63, 16, 'ticket_purchase', '-400.00', '26700.00', '26300.00', NULL, '1 szelvény vásárlása', '2026-04-07 16:19:17.834993'),
(64, 9, 'ticket_purchase', '-800.00', '432851.00', '432051.00', NULL, '2 szelvény vásárlása', '2026-04-07 16:27:41.760029'),
(65, 9, 'deposit', '1000.00', '432051.00', '433051.00', NULL, 'Egyenleg feltöltés: +1 000 Ft', '2026-04-07 16:32:00.530623'),
(66, 9, 'ticket_purchase', '-400.00', '433051.00', '432651.00', NULL, '1 szelvény vásárlása', '2026-04-07 16:55:50.844766'),
(67, 9, 'ticket_purchase', '-2000.00', '432651.00', '430651.00', NULL, '5 szelvény vásárlása', '2026-04-10 20:39:54.786035'),
(68, 12, 'deposit', '5000.00', '14300.00', '19300.00', NULL, 'Egyenleg feltöltés: +5 000 Ft', '2026-04-10 20:58:45.199537');

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `userroles`
--

CREATE TABLE `userroles` (
  `UserId` bigint(20) NOT NULL,
  `RoleId` tinyint(3) UNSIGNED NOT NULL,
  `AssignedAt` datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- A tábla adatainak kiíratása `userroles`
--

INSERT INTO `userroles` (`UserId`, `RoleId`, `AssignedAt`) VALUES
(7, 1, '2026-03-11 13:50:35.519473'),
(9, 2, '2026-03-11 17:25:14.000000'),
(10, 1, '2026-03-16 22:18:47.467546'),
(11, 1, '2026-03-16 22:33:11.743795'),
(12, 1, '2026-04-01 21:43:24.271081'),
(13, 1, '2026-04-02 20:40:18.471452'),
(14, 1, '2026-04-02 22:24:02.983273'),
(15, 1, '2026-04-04 07:51:53.975154'),
(16, 1, '2026-04-07 09:21:49.410513'),
(17, 1, '2026-04-10 23:28:15.518361');

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `users`
--

CREATE TABLE `users` (
  `Id` bigint(20) NOT NULL,
  `Username` longtext NOT NULL,
  `Email` longtext NOT NULL,
  `PasswordHash` longtext NOT NULL,
  `Balance` decimal(10,2) NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `LastLoginAt` datetime(6) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NOT NULL,
  `EmailConfirmationToken` longtext DEFAULT NULL,
  `EmailConfirmed` tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- A tábla adatainak kiíratása `users`
--

INSERT INTO `users` (`Id`, `Username`, `Email`, `PasswordHash`, `Balance`, `IsActive`, `LastLoginAt`, `CreatedAt`, `UpdatedAt`, `EmailConfirmationToken`, `EmailConfirmed`) VALUES
(7, 'TesztElek', 'tesztelek@gmail.com', '$2a$11$h3yQr083qe06pPX111uXD.8RHAzJZQ42hIO3yETX.23ErDFF6k7B.', '100.00', 1, '2026-03-11 16:11:43.157882', '2026-03-11 13:50:34.922585', '2026-03-11 13:50:34.922585', NULL, 0),
(9, 'admin', 'admin@test.com', '$2b$11$S6OegkJsfA3r2tDr3fRYkOwSBHI6lqlPNCOSutll/m86NeQxulE52', '430651.00', 1, '2026-04-11 09:30:43.517014', '2026-03-11 17:25:14.000000', '2026-04-04 07:58:04.197422', 'ec254bb8ee264cb1ad0569f4d5022b3a', 1),
(10, 'cica', 'cica123@gmail.com', '$2a$11$1ZHuqPIr59acnfTE2IQwwuDj3d1Pt/HLQ5Ft2p8rxrRlE2Po22.16', '10000.00', 1, '2026-03-16 22:19:05.046562', '2026-03-16 22:18:46.602220', '2026-03-16 22:18:46.602220', NULL, 1),
(11, 'kacsa', 'kacsa123@kacsa.com', '$2a$11$v8fyjd4FZOSLWUmlRG55P.n/a6.EZBG6sqdRIzI0YF.JI7J5Ec7hO', '10000.00', 1, '2026-03-16 22:33:25.714115', '2026-03-16 22:33:11.528657', '2026-03-16 22:33:11.528657', NULL, 1),
(12, 'maki', 'maki@gmail.com', '$2a$11$s95tDq.KvdGkt4BUs/TxK.JEwizke6Wv80lh3J79H65gC1IVkvTda', '19300.00', 1, '2026-04-10 23:20:47.889811', '2026-04-01 21:43:23.531974', '2026-04-05 18:24:06.622823', NULL, 1),
(13, 'kukac', 'kukac@gmail.com', '$2a$11$jQ7aYsw/QIdD78uoqeRaBOmSF0lmy2obBBCSZ/vjGhveXEdf3CG5O', '10000.00', 1, '2026-04-02 20:40:51.861338', '2026-04-02 20:40:18.204493', '2026-04-02 20:40:18.204493', '8587301da8034099b34618156c975c4c', 0),
(14, 'kakas', 'kakas@gmail.com', '$2a$11$24vnPTbHCVHI8kTj6d.eAOEV6R2QgyjtZXbsC5n5Jxe7fJctAkhMG', '10000.00', 1, '2026-04-02 22:24:14.849975', '2026-04-02 22:24:02.726963', '2026-04-06 23:15:13.955350', '3459f32252df407ea043bec19680a777', 0),
(15, 'sas', 'sas@gmail.com', '$2a$11$pD.ucwrA.MXAoPuafd2KQuS.8PuFtiRf.P2ryYTbxCs/LdmV2/bka', '17100.00', 1, '2026-04-04 08:58:20.700633', '2026-04-04 07:51:53.082403', '2026-04-04 07:51:53.082403', 'cc3059dbc14940a59e46a7c348e12d3f', 0),
(16, 'keselyű', 'keselyu@pelda.com', '$2a$11$9aE0yzpKdHng99pgnkBsteChm5ibe8okbQEFpcP4hUovqy6XJnGAq', '26300.00', 0, '2026-04-07 16:44:00.065523', '2026-04-07 09:21:48.807229', '2026-04-07 15:23:38.780134', '0c5aaa7acd0e49c8bdc705716b73d9d2', 0),
(17, 'kuka', 'kuka@gmail.com', '$2a$11$6u.RZq0QKB7hsQloJK.wLOx4szL68yDIsTSB0EWncPsIVl.0mCFlu', '10000.00', 1, '2026-04-10 23:28:23.938427', '2026-04-10 23:28:15.072751', '2026-04-10 23:28:15.072752', NULL, 1);

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `__efmigrationshistory`
--

CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- A tábla adatainak kiíratása `__efmigrationshistory`
--

INSERT INTO `__efmigrationshistory` (`MigrationId`, `ProductVersion`) VALUES
('20260217231946_InitialCreate', '8.0.24'),
('20260218173609_IntitalDb', '8.0.24'),
('20260316201412_AddEmailConfirmationFields', '8.0.24');

--
-- Indexek a kiírt táblákhoz
--

--
-- A tábla indexei `lotterydraws`
--
ALTER TABLE `lotterydraws`
  ADD PRIMARY KEY (`Id`);

--
-- A tábla indexei `lotterytickets`
--
ALTER TABLE `lotterytickets`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `IX_LotteryTickets_TicketCode` (`TicketCode`),
  ADD KEY `IX_LotteryTickets_DrawId` (`DrawId`),
  ADD KEY `IX_LotteryTickets_UserId` (`UserId`),
  ADD KEY `idx_ticketcode` (`TicketCode`);

--
-- A tábla indexei `roles`
--
ALTER TABLE `roles`
  ADD PRIMARY KEY (`Id`);

--
-- A tábla indexei `transactions`
--
ALTER TABLE `transactions`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_Transactions_UserId` (`UserId`);

--
-- A tábla indexei `userroles`
--
ALTER TABLE `userroles`
  ADD PRIMARY KEY (`UserId`,`RoleId`),
  ADD KEY `IX_UserRoles_RoleId` (`RoleId`);

--
-- A tábla indexei `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`Id`);

--
-- A tábla indexei `__efmigrationshistory`
--
ALTER TABLE `__efmigrationshistory`
  ADD PRIMARY KEY (`MigrationId`);

--
-- A kiírt táblák AUTO_INCREMENT értéke
--

--
-- AUTO_INCREMENT a táblához `lotterydraws`
--
ALTER TABLE `lotterydraws`
  MODIFY `Id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT a táblához `lotterytickets`
--
ALTER TABLE `lotterytickets`
  MODIFY `Id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=121;

--
-- AUTO_INCREMENT a táblához `transactions`
--
ALTER TABLE `transactions`
  MODIFY `Id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=69;

--
-- AUTO_INCREMENT a táblához `users`
--
ALTER TABLE `users`
  MODIFY `Id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=18;

--
-- Megkötések a kiírt táblákhoz
--

--
-- Megkötések a táblához `lotterytickets`
--
ALTER TABLE `lotterytickets`
  ADD CONSTRAINT `FK_LotteryTickets_LotteryDraws_DrawId` FOREIGN KEY (`DrawId`) REFERENCES `lotterydraws` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `FK_LotteryTickets_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE;

--
-- Megkötések a táblához `transactions`
--
ALTER TABLE `transactions`
  ADD CONSTRAINT `FK_Transactions_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE;

--
-- Megkötések a táblához `userroles`
--
ALTER TABLE `userroles`
  ADD CONSTRAINT `FK_UserRoles_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `FK_UserRoles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
