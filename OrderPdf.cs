using iTextSharp.text;
using iTextSharp.text.pdf;
using PizzaShop.Entity.ViewModels;
using Doc = iTextSharp.text.Document;  
namespace PizzaShop.Entity.Helpers;

public class PdfHelper
{
    public static byte[] GeneratePdf(OrderDetailsViewModel model)
    {

        using (var ms = new MemoryStream())
        {
                // Fully qualify the Document class
            Doc document = new Doc();
            PdfWriter.GetInstance(document, ms);
            document.Open();

            BaseColor blueColor = new BaseColor(0, 102, 167); 

            // Create a bold, blue heading font
            Font headingFont = FontFactory.GetFont("Arial", 14, Font.BOLD, blueColor);
            Font headingFont1 = FontFactory.GetFont("Arial", 22, Font.BOLD, blueColor);

            string imagePath = "C:/Users/pctr48/Downloads/New folder (3)/PizzaShop/PizzaShop.Web/wwwroot/images1/logos/pizzashop_logo.png";

            //photo and pizzashop heading section
            PdfPTable photoheadingtable = new PdfPTable(2);
            photoheadingtable.WidthPercentage = 40; 

            // Set equal width for both columns
            float[] widths1 = new float[] { 1, 3 }; 
            photoheadingtable.SetWidths(widths1);

            // Add Image
            Image img = Image.GetInstance(imagePath);
            img.ScaleToFit(50, 50); 
            PdfPCell imageCell = new PdfPCell(img);
            imageCell.Border = Rectangle.NO_BORDER; 
            imageCell.HorizontalAlignment = Element.ALIGN_CENTER;
            imageCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            photoheadingtable.AddCell(imageCell);

            // Add Heading
            Paragraph heading = new Paragraph("PIZZASHOP", headingFont1);
            PdfPCell textCell = new PdfPCell(heading);
            textCell.Border = Rectangle.NO_BORDER; 
            textCell.HorizontalAlignment = Element.ALIGN_CENTER;
            textCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            photoheadingtable.AddCell(textCell);
            document.Add(photoheadingtable);

            Paragraph headerspace = new Paragraph("");
            headerspace.SpacingBefore = 30f; 
            document.Add(headerspace); 

            //customr and order section
            PdfPTable customerordertable = new PdfPTable(2);
            customerordertable.WidthPercentage = 100; 

            // Set relative widths for columns (adjust as needed)
            float[] widths = new float[] { 1f, 1f }; 
            customerordertable.SetWidths(widths);

            // ---- Customer Details Column ----
            PdfPTable customerTable = new PdfPTable(1);
            customerTable.AddCell(new PdfPCell(new Phrase("Customer Details", headingFont)) { Border = Rectangle.NO_BORDER });
            customerTable.AddCell(new PdfPCell(new Phrase($"Name: {model.CustomerName}")) { Border = Rectangle.NO_BORDER });
            customerTable.AddCell(new PdfPCell(new Phrase($"Mob: {model.Phone}")) { Border = Rectangle.NO_BORDER });

            // ---- Order Details Column ----
            PdfPTable orderTable = new PdfPTable(1);
            orderTable.AddCell(new PdfPCell(new Phrase("Order Details", headingFont)) { Border = Rectangle.NO_BORDER });
            orderTable.AddCell(new PdfPCell(new Phrase($"Invoice Number: {model.OrderId}")) { Border = Rectangle.NO_BORDER });
            orderTable.AddCell(new PdfPCell(new Phrase($"Date: {model.OrderDate:dd-MM-yyyy HH:mm}")) { Border = Rectangle.NO_BORDER });
            orderTable.AddCell(new PdfPCell(new Phrase($"Section: {model.Section}")) { Border = Rectangle.NO_BORDER });
            orderTable.AddCell(new PdfPCell(new Phrase($"Table: {model.TableName}")) { Border = Rectangle.NO_BORDER });

            // Add both tables to the main table 
            // Add Customer Table  and Order Table 
            PdfPCell customerCell = new PdfPCell(customerTable)
            {
                Border = Rectangle.NO_BORDER  
            };
            customerordertable.AddCell(customerCell);
            PdfPCell orderCell = new PdfPCell(orderTable)
            {
                Border = Rectangle.NO_BORDER  
            };
            customerordertable.AddCell(orderCell);
            document.Add(customerordertable);
            Paragraph space = new Paragraph("");
            space.SpacingBefore = 20f;
            document.Add(space);


            //table header
            PdfPTable table = new PdfPTable(5)
            {
                WidthPercentage = 100 
            };
            table.SetWidths(new float[] { 10f, 45f, 15f, 15f, 15f }); 
            AddTableHeader(table);

            // Add Table Data
            int serialNo = 1;
            foreach (var item in model.Items)
            {
                // Item Row
                table.AddCell(CreateCell(serialNo++.ToString(), false)); 
                table.AddCell(CreateCell(item.Name, false));             
                table.AddCell(CreateCell(item.Quantity.ToString(), false)); 
                table.AddCell(CreateCell(item.ItemPrice.ToString("C"), false)); 
                table.AddCell(CreateCell(item.TotalAmount.ToString("C"), false)); 

                // Item Modifiers (if any)
                if (item.ItemModifier != null && item.ItemModifier.Any())
                {
                    int modifierCount = item.ItemModifier.Count;
                    int currentIndex = 0; 

                    foreach (var modifier in item.ItemModifier)
                    {
                        bool isLastModifier = currentIndex == modifierCount - 1; 

                        // Add cells with bottom border only for the last modifier
                        table.AddCell(CreateCell("", isLastModifier)); 
                        table.AddCell(CreateCell(modifier.Name, isLastModifier));
                        table.AddCell(CreateCell(modifier.Quantity.ToString(), isLastModifier)); 
                        table.AddCell(CreateCell(modifier.ItemRate.ToString("C"), isLastModifier));
                        table.AddCell(CreateCell(modifier.ItemAmount.ToString("C"), isLastModifier));

                        currentIndex++; 
                    }
                }

            }

            document.Add(table); 

            //subtotal
            PdfPTable subtotal = new PdfPTable(2);
            subtotal.WidthPercentage = 100; 
            subtotal.SetWidths(new float[] { 1, 1 }); 

            // Add "Sub Total" label (left-aligned)
            PdfPCell labelCell = new PdfPCell(new Phrase("Sub Total:"));
            labelCell.Border = Rectangle.NO_BORDER; 
            labelCell.HorizontalAlignment = Element.ALIGN_LEFT; 
            subtotal.AddCell(labelCell);

            // Add the subtotal value (right-aligned)
            PdfPCell valueCell = new PdfPCell(new Phrase(model.Subtotal.ToString("C")));
            valueCell.Border = Rectangle.NO_BORDER;
            valueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            subtotal.AddCell(valueCell);
            subtotal.SpacingBefore = 15f;
            document.Add(subtotal);

            foreach (var tax in model.TaxDetails)
            {
                //tax table
                PdfPTable taxTable3 = new PdfPTable(2);
                taxTable3.WidthPercentage = 100;
                taxTable3.SetWidths(new float[] { 1, 1 }); 

                // Tax Name (left-aligned)
                PdfPCell TAXNAME = new PdfPCell(new Phrase($"{tax.TaxName}:"));
                TAXNAME.Border = Rectangle.NO_BORDER;
                TAXNAME.HorizontalAlignment = Element.ALIGN_LEFT;
                taxTable3.AddCell(TAXNAME);

                // Tax Value (right-aligned)
                PdfPCell TAXVALUE = new PdfPCell(new Phrase($"₹{tax.TaxValue}"));
                TAXVALUE.Border = Rectangle.NO_BORDER;
                TAXVALUE.HorizontalAlignment = Element.ALIGN_RIGHT;
                taxTable3.AddCell(TAXVALUE);
                document.Add(taxTable3);    
            }

            //one border line
            PdfPTable table2 = new PdfPTable(1);
            table2.WidthPercentage = 100; 
            table2.SpacingBefore = 14f;
            table2.SpacingAfter = 10f; 
            PdfPCell cell = new PdfPCell(new Phrase(""));
            cell.BorderWidthBottom = 1f; 
            cell.BorderColorBottom = new BaseColor(0, 102, 167); 
            table2.AddCell(cell);
            document.Add(table2);


            // Totaltable
            PdfPTable totalTable = new PdfPTable(2);
            totalTable.WidthPercentage = 100; 
            totalTable.SetWidths(new float[] { 1, 1 }); 

            // Total Label (left-aligned)
            PdfPCell labelCell1 = new PdfPCell(new Phrase("Total Amount Due:", headingFont));
            labelCell1.Border = Rectangle.NO_BORDER;
            labelCell1.HorizontalAlignment = Element.ALIGN_LEFT;
            totalTable.AddCell(labelCell1);

            // Total Amount (right-aligned)
            PdfPCell amountCell = new PdfPCell(new Phrase($"₹{model.TotalAmount}", headingFont));
            amountCell.Border = Rectangle.NO_BORDER;
            amountCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalTable.AddCell(amountCell);
            document.Add(totalTable);

            //paymentinformation
            Paragraph payment = new Paragraph($"Payment Information",headingFont);
            payment.SpacingBefore = 15f; 
            document.Add(payment);
            Paragraph paymentmode = new Paragraph($"Payment Method: {model.Paymentmode}");
            paymentmode.SpacingAfter = 10f; 
            document.Add(paymentmode);

            Paragraph thankYou = new Paragraph("THANK YOU!", headingFont);
            thankYou.Alignment = Element.ALIGN_CENTER; 
            thankYou.SpacingBefore = 10f; 
            thankYou.SpacingAfter = 10f; 
            document.Add(thankYou);

            document.Close();
            return ms.ToArray();
        }
    }


     private static void AddTableHeader(PdfPTable table)
    {
        // Define header font
        BaseColor headerColor = new BaseColor(0, 102, 167);
        Font headerFont = FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE);

        // Add Headers
        AddHeaderCell(table, "Sr.No", headerFont, headerColor);
        AddHeaderCell(table, "Item", headerFont, headerColor);
        AddHeaderCell(table, "Quantity", headerFont, headerColor);
        AddHeaderCell(table, "Unit Price", headerFont, headerColor);
        AddHeaderCell(table, "Total", headerFont, headerColor);
    }

    private static void AddHeaderCell(PdfPTable table, string text, Font font, BaseColor bgColor)
    {
        PdfPCell cell = new PdfPCell(new Phrase(text, font))
        {
            BackgroundColor = bgColor,
            Padding = 5,
            Border = Rectangle.NO_BORDER // Remove border
        };
        table.AddCell(cell);
    }

    private static PdfPCell CreateCell(string text, bool addBottomBorder = false)
    {
        var cell = new PdfPCell(new Phrase(text))
        {
            Border = Rectangle.NO_BORDER, // No border by default
            Padding = 5,                 // Optional padding
        };

        // Add a blue bottom border if required
        if (addBottomBorder)
        {
            cell.BorderWidthBottom = 2;             
            cell.BorderColorBottom = new BaseColor(190, 253, 253); 
            cell.PaddingBottom = 15f;
        }

        return cell;
    }


}
