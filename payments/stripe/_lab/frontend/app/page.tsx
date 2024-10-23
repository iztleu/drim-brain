import Link from "next/link";
import { Container, NavLink, Title } from "@mantine/core";

export default function Home() {
  return (
    <Container>
      <Title my="xl">StripeLab</Title>
      <NavLink href="/products" label="Products"></NavLink>
    </Container>
  );
}
