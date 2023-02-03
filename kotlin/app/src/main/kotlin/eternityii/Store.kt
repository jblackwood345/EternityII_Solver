package eternityii

import java.io.File
import java.math.BigInteger
import java.nio.file.FileAlreadyExistsException
import java.nio.file.Files
import java.nio.file.Paths
import java.security.MessageDigest
import java.util.UUID
import kotlin.random.Random

object Store {
    fun saveBoard(board: List<RotatedPiece>, maxSolveIndex: UShort) {
        val entireBoard = StringBuilder()
        val url = StringBuilder()

        for (i in 15 downTo 0) {
            val row = StringBuilder()
            for (j in 0..15) {
                if (board[i * 16 + j].pieceNumber > 0U) {
                    val pieceId = ((board[i * 16 + j].pieceNumber)).toString().padStart(3, ' ')
                    val rotation = board[i * 16 + j].rotations
                    row.append("$pieceId/$rotation ")

                    var foundPiece: Piece? = null

                    pieces.forEach { testPiece ->
                        if (testPiece.pieceNumber == board[i * 16 + j].pieceNumber) {
                            foundPiece = testPiece
                        }
                    }
                    val piece = foundPiece!!

                    when (board[i * 16 + j].rotations.toInt()) {
                        0 -> {
                            url.append('a' + piece.topSide.toInt())
                            url.append('a' + piece.rightSide.toInt())
                            url.append('a' + piece.bottomSide.toInt())
                            url.append('a' + piece.leftSide.toInt())
                        }

                        1 -> {
                            url.append('a' + piece.leftSide.toInt())
                            url.append('a' + piece.topSide.toInt())
                            url.append('a' + piece.rightSide.toInt())
                            url.append('a' + piece.bottomSide.toInt())
                        }

                        2 -> {
                            url.append('a' + piece.bottomSide.toInt())
                            url.append('a' + piece.leftSide.toInt())
                            url.append('a' + piece.topSide.toInt())
                            url.append('a' + piece.rightSide.toInt())
                        }

                        3 -> {
                            url.append('a' + piece.rightSide.toInt())
                            url.append('a' + piece.bottomSide.toInt())
                            url.append('a' + piece.leftSide.toInt())
                            url.append('a' + piece.topSide.toInt())
                        }
                    }
                } else {
                    row.append("---/- ")
                    url.append("aaaa")
                }
            }

            row.append("\n")
            entireBoard.append(row.toString())
        }

        val boardString = entireBoard.toString() +
            "\nhttps://e2.bucas.name/#puzzle=Joshua_Blackwood&board_w=16&board_h=16&board_edges=$url" +
            "&motifs_order=jblackwood"

        val messageDigest = MessageDigest.getInstance("MD5")
        val bytes = BigInteger(1, messageDigest.digest(boardString.toByteArray())).toByteArray()
        val uuid = UUID.nameUUIDFromBytes(bytes)
        val filename = "${maxSolveIndex}_${uuid}_${Random.Default.nextInt(0, 1000000)}.txt"

        val path = System.getProperty("user.home") + "/EternitySolutions"
        try {
            Files.createDirectory(Paths.get(path))
        } catch (e: FileAlreadyExistsException) {
            // Fine.
        }
        File("$path/$filename").writeText("$boardString\n")
    }
}
