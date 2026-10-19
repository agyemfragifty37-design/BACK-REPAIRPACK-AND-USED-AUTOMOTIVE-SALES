Public Class Form1

    ' Rates
    Private Const decDISCOUNT_RATE As Decimal = 0.2D
    Private Const decTAX_RATE As Decimal = 0.05D
    ' Exterior extras (same order as the lstExterior items)


    Private Const decPAINT_TOUCH_UP As Decimal = 250D
    Private Const decUNDERCOAT As Decimal = 300D
    Private Const decBOTH As Decimal = 550D


    ' Accessory extras (same order as the clbAccessories items)
    Private Const decNEW_TIRES As Decimal = 450D
    Private Const decHD_RADIO As Decimal = 190.95D
    Private Const decGPS As Decimal = 700D
    Private Const decFLOOR_MATS As Decimal = 55D
    ' Oldest model year shown in the year list


    Private Const intOLDEST_YEAR As Integer = 1990


    ' Module-level accumulator for the grand total of all sales


    Private decGrandTotalDue As Decimal = 0D
    ' Fills the year list and sets the starting values.

    Private Sub Lab4_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim intYear As Integer
        ' For...Next loop counting DOWN: newest year first.

        For intYear = Now.Year To intOLDEST_YEAR Step -1
            cboYear.Items.Add(intYear.ToString())
        Next
        ResetForm()
    End Sub
    ' Clears the form. Used by Form Load and the Reset button.

    Private Sub ResetForm()
        Dim intIndex As Integer
        txtLotNumber.Clear()
        cboYear.SelectedIndex = -1 ' No year selected
        txtMakeModel.Clear()
        chkWholesale.Checked = False
        txtPrice.Clear()
        txtTradeIn.Text = "0.00"
        txtDiscount.Clear()
        txtExtras.Clear()
        txtSubtotal.Clear()
        txtSalesTax.Clear()
        txtTotalDue.Clear()
        lstExterior.SelectedIndex = 0 ' Select "None"

        ' Loop through the accessories and uncheck each one.

        For intIndex = 0 To clbAccessories.Items.Count - 1
            clbAccessories.SetItemChecked(intIndex, False)
        Next
        txtLotNumber.Focus()
    End Sub

    ' Validates the input, computes the sale and records it.


    Private Sub btnCompute_Click(sender As Object, e As EventArgs) Handles btnCompute.Click
        Dim decPrice As Decimal
        Dim decTradeIn As Decimal
        Dim decDiscount As Decimal
        Dim decExtras As Decimal = 0D ' Accumulator for all extras
        Dim decSubtotal As Decimal
        Dim decSalesTax As Decimal
        Dim decTotalDue As Decimal
        Dim intIndex As Integer
        Try

            ' ----- Business rules: If-ElseIf-Else stops at the first failure -----

            If txtLotNumber.Text.Trim() = String.Empty Then
                MessageBox.Show("Lot number is missing", "Data Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtLotNumber.Focus()
            ElseIf cboYear.SelectedIndex = -1 Then
                MessageBox.Show("Year is missing", "Data Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error)
                cboYear.Focus()
            ElseIf txtMakeModel.Text.Trim() = String.Empty Then
                MessageBox.Show("Vehicle make/model is missing", "Data Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtMakeModel.Focus()
            ElseIf Not Decimal.TryParse(txtPrice.Text, decPrice) OrElse decPrice <=
       0D Then
                MessageBox.Show("Price must be a number greater than zero", "Data 
Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtPrice.Focus()
                txtPrice.SelectAll()
            ElseIf Not Decimal.TryParse(txtTradeIn.Text, decTradeIn) OrElse
       decTradeIn < 0D Then
                MessageBox.Show("Trade-in must be a number and cannot be less than 
zero",
            "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtTradeIn.Focus()
                txtTradeIn.SelectAll()
            ElseIf LotAlreadySold(txtLotNumber.Text.Trim()) Then
                MessageBox.Show("Lot number " & txtLotNumber.Text.Trim() &
            " has already been sold", "Data Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtLotNumber.Focus()
                txtLotNumber.SelectAll()
            Else
                ' ----- Wholesale discount (price only) -----

                If chkWholesale.Checked Then
                    decDiscount = Decimal.Round(decPrice * decDISCOUNT_RATE, 2)
                Else
                    decDiscount = 0D
                End If
                ' ----- Exterior extra: one choice from the ListBox -----

                If lstExterior.SelectedIndex = 1 Then
                    decExtras += decPAINT_TOUCH_UP
                ElseIf lstExterior.SelectedIndex = 2 Then
                    decExtras += decUNDERCOAT
                ElseIf lstExterior.SelectedIndex = 3 Then
                    decExtras += decBOTH
                End If ' Index 0 (None) adds nothing

                ' ----- Accessory extras: loop through the CheckedListBox -----

                For intIndex = 0 To clbAccessories.Items.Count - 1
                    If clbAccessories.GetItemChecked(intIndex) Then
                        If intIndex = 0 Then
                            decExtras += decNEW_TIRES
                        ElseIf intIndex = 1 Then
                            decExtras += decHD_RADIO
                        ElseIf intIndex = 2 Then
                            decExtras += decGPS
                        Else
                            decExtras += decFLOOR_MATS
                        End If
                    End If
                Next
                ' ----- Subtotal, sales tax and total due -----

                decSubtotal = decPrice - decDiscount + decExtras
                If chkWholesale.Checked Then
                    decSalesTax = 0D ' Dealers pay no tax
                Else
                    decSalesTax = Decimal.Round(decSubtotal * decTAX_RATE, 2)
                End If
                decTotalDue = decSubtotal + decSalesTax - decTradeIn

                ' ----- Display the results -----

                txtDiscount.Text = decDiscount.ToString("N")
                txtExtras.Text = decExtras.ToString("N")
                txtSubtotal.Text = decSubtotal.ToString("N")
                txtSalesTax.Text = decSalesTax.ToString("N")
                txtTotalDue.Text = decTotalDue.ToString("C")

                ' ----- Record the sale -----

                decGrandTotalDue += decTotalDue
                lstSales.Items.Add(txtLotNumber.Text.Trim() & " | " &
            cboYear.Text & " " & txtMakeModel.Text.Trim() & " | " &
            decTotalDue.ToString("C"))
            End If
        Catch ex As Exception
            MessageBox.Show("Unexpected error: " & ex.Message, "Error",
        MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Returns True if the lot number is already in the Sales History list.

    Private Function LotAlreadySold(strLot As String) As Boolean
        Dim intIndex As Integer = 0
        Dim blnFound As Boolean = False
        Do While intIndex < lstSales.Items.Count AndAlso Not blnFound
            If lstSales.Items(intIndex).ToString().StartsWith(strLot & " |") Then
                blnFound = True
            Else
                intIndex += 1
            End If
        Loop
        Return blnFound
    End Function

    ' Clears the form for the next sale.

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        ResetForm()
    End Sub

    ' Shows the grand total, number sold and average total due.

    Private Sub btnTotals_Click(sender As Object, e As EventArgs) Handles btnTotals.Click
        Dim intVehiclesSold As Integer
        Dim decAverage As Decimal
        Try
            intVehiclesSold = lstSales.Items.Count
            decAverage = decGrandTotalDue / intVehiclesSold ' Throws if 0 sold
            MessageBox.Show("Total due all sales: " & decGrandTotalDue.ToString("C") &
 ControlChars.NewLine & "Total vehicles sold: " &
intVehiclesSold.ToString() &
 ControlChars.NewLine & "Average total due: " &
decAverage.ToString("C"),
 "Sales Totals", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As DivideByZeroException
            MessageBox.Show("No vehicles have been sold yet.", "No Sales",
            MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Unexpected error: " & ex.Message, "Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Closes the form only if the user confirms. No is the default.

    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        If MessageBox.Show("Do you want to exit?", "Quit?",
 MessageBoxButtons.YesNo, MessageBoxIcon.Question,
 MessageBoxDefaultButton.Button2) = DialogResult.Yes Then
            Me.Close()
        End If
    End Sub

End Class