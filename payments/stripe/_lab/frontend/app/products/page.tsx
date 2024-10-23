'use client';

import React, {useState, useEffect} from 'react';
import { Card, Text, Title, Grid, Container } from '@mantine/core';

interface Product {
    id: string;
    name: string;
    description: string;
    price: number;
    stockQuantity: number;
}

export default function() {
    const [products, setProducts] = useState<Product[]>([]);

    const API_URL = process.env.NEXT_PUBLIC_API_URL;

    useEffect(() => {
        async function fetchProducts() {
            let res = await fetch(`${API_URL}/products`)
            let data = await res.json()
            setProducts(data)
        }
        fetchProducts()
    }, []);
    
    return (
        <Container>
          <Title my="xl">Products</Title>
          <Grid gutter="xl">
            {products.map((product) => (
            <Grid.Col span={12}>
              <Card key={product.id} shadow="xs" padding="xl" radius="lg">
                <Title order={3}>{product.name}</Title>
                <Text>{product.description}</Text>
                <Text>${product.price}</Text>
                <Text>Stock: {product.stockQuantity}</Text>
              </Card>
            </Grid.Col>
            ))}
          </Grid>
        </Container>
      );
}
