package org.example.springboot.controller;

import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.ResponseBody;

@Controller
@RequestMapping("/product")
public class ProductController {
    @GetMapping({ "/detail/{id}"})
    @ResponseBody
    public String getProductDetail(@PathVariable("id") String id) {
        try {
            int productId = Integer.parseInt(id);
            if (productId <= 0) { return "Error: Product ID greater than 0"; }
            return "Product ID = " + productId;
        } catch (NumberFormatException e) { return "Error: Product ID invalid (must be integer)"; }
    }

    @GetMapping("/category")
    @ResponseBody
    public String getCategory(@RequestParam(value = "name", required = false) String name) {
        if (name == null || name .trim().isEmpty()) { return "Error: Missing 'name' parameter or invalid category name!"; }
        return "Category = " + name;
    }
}
