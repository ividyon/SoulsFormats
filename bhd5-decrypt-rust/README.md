This is a simple implementation of RSA public key decryption written in Rust intended to help decrypt .bhd files. The latest built .dll should always be included in the C# directory, so you shouldn't need to touch this unless you want to mess with the RSA decryption specifically, or you want to build for a platform other than Windows.

To build the library, run this command:

```
cargo build --lib --release
```

The .dll will be in `/target/release`, and should be copied to `../SoulsFormats/` to be used with the C# code.

A small `main.rs` file is also included, used for testing the library directly from Rust. Real tests should be created in the future. 

`main.rs` can be run with this command:

```
cargo run --release
```