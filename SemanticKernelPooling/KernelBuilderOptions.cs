namespace SemanticKernelPooling
{
    /// <summary>
    /// Represents options for configuring the kernel builder within the kernel pool.
    /// </summary>
    /// <remarks>
    /// Allows users to specify whether to automatically register the chat completion service and to provide a custom
    /// <see cref="HttpClient"/> instance if the default one should be replaced.
    /// </remarks>
    public class KernelBuilderOptions
    {
        /// <summary>
        /// Gets or sets the <see cref="HttpClient"/> to use for the kernel.
        /// </summary>
        /// <remarks>
        /// If set to <c>null</c>, the default HTTP client provided by the kernel will be used.
        /// This allows for customization of HTTP handling, such as adding custom headers or changing the base address.
        /// </remarks>
        public HttpClient? HttpClient { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the chat completion service should be automatically added to the kernel.
        /// </summary>
        /// <remarks>
        /// If set to <c>true</c>, the chat completion service will be automatically registered with the kernel builder.
        /// If set to <c>false</c>, the service must be manually registered by the user.
        /// </remarks>
        public bool ShouldAutoAddChatCompletionService { get; set; } = true;
    }
}